using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SaturnGame.Data;
using SaturnGame.Loading;
using UnityEngine;

namespace SaturnGame.UI
{
public class SongSelectLogic : MonoBehaviour
{
    [SerializeField] private SongSelectCardAnimator cardAnimator;
    [SerializeField] private SongSelectPageAnimator pageAnimator;
    [SerializeField] private SongSelectDisplayAnimator displayAnimator;
    [SerializeField] private SongDatabase songDatabase;
    [SerializeField] private ButtonPageManager buttonManager;
    [SerializeField] private BgmPreviewController bgmPreview;

    [SerializeField] private GroupType selectedGroupType;
    [SerializeField] private SortType selectedSortType;
    private SortedSongList songList;
    [SerializeField] private int selectedGroupIndex;
    [SerializeField] private int selectedEntryIndex;
    private SongListEntry SelectedEntry => songList.Groups[selectedGroupIndex].Entries[selectedEntryIndex];
    [SerializeField] private Difficulty selectedDifficulty;
    private SongDifficulty SelectedDifficultInfo => SelectedEntry.Song.SongDiffs[selectedDifficulty];

    private enum MenuPage
    {
        SongSelect = 0,
        ChartPreview = 1,
        ExitingMenu = 2,
    }

    private MenuPage page = MenuPage.SongSelect;

    [SerializeField] private GameObject diffPlusButton0;
    [SerializeField] private GameObject diffPlusButton1;
    [SerializeField] private GameObject diffMinusButton0;
    [SerializeField] private GameObject diffMinusButton1;
    private static UIAudioController UIAudio => UIAudioController.Instance;

    private async void Start()
    {
        Debug.Log($"Coming from {SceneSwitcher.Instance.LastScene}");
        page = SceneSwitcher.Instance.LastScene == "_Options" ? MenuPage.ChartPreview : MenuPage.SongSelect;

        songDatabase.LoadAllSongData();

        SetSongAndDiffFromPersistentState();

        if (page is MenuPage.ChartPreview)
        {
            buttonManager.SetActiveButtons(1);
            pageAnimator.ToChartPreviewInstant();
        }

        await LoadAllCards();
    }

    private void SetSongAndDiffFromPersistentState()
    {
        SetSortType(PersistentStateManager.Instance.SelectedGroupType,
            PersistentStateManager.Instance.SelectedSortType);
        int groupIndex = 0;
        int entryIndex = 0;
        Difficulty difficulty = 0;

        if (PersistentStateManager.Instance.SelectedSong.FolderPath is string path)
        {
            (int, int)? foundIndexes =
                songList.FindSongFolder(path, PersistentStateManager.Instance.SelectedDifficulty.Difficulty);

            if (foundIndexes != null)
            {
                (groupIndex, entryIndex) = foundIndexes.Value;
                // We aren't guaranteed that this difficulty still exists on this song, but SetSongAndDifficulty will
                // find the nearest difficulty in case this one no longer exists, so it should be fine.
                // WARNING: This assumes that the difficulty index is the same as the enum value.
                difficulty = PersistentStateManager.Instance.SelectedDifficulty.Difficulty;
            }
        }

        SetSongAndDifficulty(groupIndex, entryIndex, difficulty);
    }

    private void SetSortType(GroupType groupType, SortType sortType)
    {
        selectedGroupType = groupType;
        selectedSortType = sortType;
        PersistentStateManager.Instance.SelectedGroupType = selectedGroupType;
        PersistentStateManager.Instance.SelectedSortType = selectedSortType;

        songList = songDatabase.SortSongList(selectedGroupType, selectedSortType);
    }

    private async Awaitable ChangeSortType(GroupType groupType, SortType sortType)
    {
        Song currentSong = SelectedEntry.Song;
        Difficulty currentDifficulty = selectedDifficulty;

        SetSortType(groupType, sortType);

        (int newGroupIndex, int newEntryIndex) =
            songList.FindSongFolder(currentSong.FolderPath, currentDifficulty) ?? (0, 0);

        SetSongAndDifficulty(newGroupIndex, newEntryIndex, currentDifficulty);

        await LoadAllCards();
    }

    private void SetSongAndDifficulty(int groupIndex, int entryIndex, Difficulty difficulty)
    {
        selectedGroupIndex = groupIndex;
        selectedEntryIndex = entryIndex;
        PersistentStateManager.Instance.SelectedSong = SelectedEntry.Song;
        // Always set difficulty after setting the song to avoid leaving difficulty set to a value that is not
        // valid for the current song.
        SetDifficulty(difficulty);
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        Dictionary<Difficulty, SongDifficulty> diffInfos = SelectedEntry.Song.SongDiffs;
        selectedDifficulty = FindNearestDifficulty(diffInfos.Keys, difficulty);
        PersistentStateManager.Instance.SelectedDifficulty = SelectedEntry.Song.SongDiffs[selectedDifficulty];

        diffPlusButton0.SetActive(HigherDiffExists(diffInfos, selectedDifficulty));
        diffPlusButton1.SetActive(HigherDiffExists(diffInfos, selectedDifficulty));
        diffMinusButton0.SetActive(LowerDiffExists(diffInfos, selectedDifficulty));
        diffMinusButton1.SetActive(LowerDiffExists(diffInfos, selectedDifficulty));

        displayAnimator.SetSongData(SelectedEntry.Song, selectedDifficulty);

        SetBgmValues();
    }


    public async void OnDifficultyChange(int changeBy)
    {
        if (page == MenuPage.ExitingMenu) return;
        if (selectedDifficulty + changeBy is < 0 or > Difficulty.Beyond) return;

        Difficulty prevDifficulty = selectedDifficulty;
        SetDifficulty(selectedDifficulty + changeBy);

        if (prevDifficulty == selectedDifficulty) return;

        Awaitable awaitable = null;

        if (!SelectedEntry.Difficulties.Contains(selectedDifficulty))
        {
            // Switch to the new entry for this diff.
            (int, int)? indexes = songList.FindSongFolder(SelectedEntry.Song.FolderPath, selectedDifficulty);

            // If indexes is null, the pattern will not match.
            if (indexes is var (groupIndex, entryIndex))
            {
                SetSongAndDifficulty(groupIndex, entryIndex, selectedDifficulty);

                awaitable = LoadAllCards();
            }
            else
                Debug.LogWarning($"Couldn't find entry for {selectedDifficulty}");
        }

        UIAudio.PlaySound(UIAudioController.UISound.Navigate);

        if (page is not MenuPage.ChartPreview) return;

        bgmPreview.FadeoutBgmPreview();
        bgmPreview.ResetLingerTimer();

        if (awaitable != null) await awaitable;
    }

    public void OnBack()
    {
        if (page == MenuPage.ExitingMenu) return;
        UIAudio.PlaySound(UIAudioController.UISound.Back);

        switch (page)
        {
            case MenuPage.SongSelect:
            {
                page = MenuPage.ExitingMenu;
                bgmPreview.FadeoutBgmPreview(true);
                SceneSwitcher.Instance.LoadScene("_TitleScreen");
                break;
            }
            case MenuPage.ChartPreview:
            {
                page = MenuPage.SongSelect;

                pageAnimator.Anim_ToSongSelect();
                buttonManager.SwitchButtons(0);
                break;
            }
            case MenuPage.ExitingMenu:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void OnConfirm()
    {
        switch (page)
        {
            case MenuPage.ExitingMenu:
            {
                break;
            }
            case MenuPage.SongSelect:
            {
                UIAudio.PlaySound(UIAudioController.UISound.Impact);

                bgmPreview.FadeoutBgmPreview();
                bgmPreview.ResetLingerTimer();

                page = MenuPage.ChartPreview;
                pageAnimator.Anim_ToChartPreview();
                buttonManager.SwitchButtons(1);
                break;
            }
            case MenuPage.ChartPreview:
            {
                UIAudio.PlaySound(UIAudioController.UISound.StartGame);
                bgmPreview.FadeoutBgmPreview(true);
                page = MenuPage.ExitingMenu;

                SceneSwitcher.Instance.LoadScene("_RhythmGame");
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }


    private enum NavigateDirection
    {
        Left = -1,
        Right = +1,
    }

    private async Awaitable OnNavigateLeftRight(NavigateDirection direction)
    {
        if (page == MenuPage.ExitingMenu) return;
        if (page is not MenuPage.SongSelect) return;

        UIAudio.PlaySound(UIAudioController.UISound.Navigate);

        (int newGroupIndex, int newEntryIndex) = songList
            .RelativeSongIndexes(selectedGroupIndex, selectedEntryIndex, (int)direction);
        Difficulty newDifficulty =
            FindNearestDifficulty(songList.Groups[newGroupIndex].Entries[newEntryIndex].Difficulties,
                selectedDifficulty);
        SetSongAndDifficulty(newGroupIndex, newEntryIndex, newDifficulty);

        // Update Cards
        SongSelectCardAnimator.ShiftDirection shiftDirection = direction switch
        {
            // If you move left, the cards shift right, and vice versa.
            NavigateDirection.Left => SongSelectCardAnimator.ShiftDirection.Right,
            NavigateDirection.Right => SongSelectCardAnimator.ShiftDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        cardAnimator.Anim_ShiftCards(shiftDirection);
        SongListEntry newSongEntry = songList.RelativeSongEntry(selectedGroupIndex, selectedEntryIndex,
            cardAnimator.CardHalfCount * (int)direction);
        Awaitable loadNewJacket = LoadCardJacket(cardAnimator.WrapCardIndex, newSongEntry.Song.JacketPath);

        cardAnimator.SetSongData(cardAnimator.WrapCardIndex,
            FindNearestDifficulty(newSongEntry.Difficulties, selectedDifficulty), newSongEntry.Song);

        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

        // Audio Preview
        bgmPreview.FadeoutBgmPreview();
        bgmPreview.ResetLingerTimer();

        // Await the jacket which has been loading in the background to make sure we rethrow any errors
        await loadNewJacket;
    }

    public async void OnNavigateLeft()
    {
        await OnNavigateLeftRight(NavigateDirection.Left);
    }


    public async void OnNavigateRight()
    {
        await OnNavigateLeftRight(NavigateDirection.Right);
    }

    private async Awaitable OnGroupLeftRight(NavigateDirection direction)
    {
        if (page == MenuPage.ExitingMenu) return;
        if (page is not MenuPage.SongSelect) return;

        UIAudio.PlaySound(UIAudioController.UISound.Navigate);

        int newGroupIndex = SaturnMath.Modulo(selectedGroupIndex + (int)direction, songList.Groups.Count);
        int newEntryIndex = direction switch
        {
            NavigateDirection.Left => songList.Groups[newGroupIndex].Entries.Count - 1,
            NavigateDirection.Right => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        SongListEntry newEntry = songList.Groups[newGroupIndex].Entries[newEntryIndex];
        Difficulty newDifficulty = FindNearestDifficulty(newEntry.Difficulties, selectedDifficulty);
        SetSongAndDifficulty(newGroupIndex, newEntryIndex, newDifficulty);
        Debug.Log($"Jumped to {songList.Groups[selectedGroupIndex].Name}");

        // We have to update all the card jackets since we might be shifting by a lot, but make the cards shift anyway
        // so that the movement looks slightly more natural.
        SongSelectCardAnimator.ShiftDirection shiftDirection = direction switch
        {
            // If you move left, the cards shift right, and vice versa.
            NavigateDirection.Left => SongSelectCardAnimator.ShiftDirection.Right,
            NavigateDirection.Right => SongSelectCardAnimator.ShiftDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
        cardAnimator.Anim_ShiftCards(shiftDirection);

        Awaitable loadCards = LoadAllCards();

        // Audio Preview
        bgmPreview.FadeoutBgmPreview();
        bgmPreview.ResetLingerTimer();

        await loadCards;
        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());
    }

    public async void OnGroupLeft()
    {
        await OnGroupLeftRight(NavigateDirection.Left);
    }

    public async void OnGroupRight()
    {
        await OnGroupLeftRight(NavigateDirection.Right);
    }

    public async void OnSort()
    {
        // Since we are missing a more sophisticated UI, for now just link the group and sort options together.
        // Pressing the sort button will cycle through the options.
        switch (selectedSortType)
        {
            case SortType.Title:
            {
                await ChangeSortType(GroupType.Artist, SortType.Artist);
                break;
            }
            case SortType.Artist:
            {
                await ChangeSortType(GroupType.Charter, SortType.Charter);
                break;
            }
            case SortType.Charter:
            {
                await ChangeSortType(GroupType.All, SortType.Bpm);
                break;
            }
            case SortType.Bpm:
            {
                await ChangeSortType(GroupType.Level, SortType.Level);
                break;
            }
            case SortType.Level:
            {
                await ChangeSortType(GroupType.All, SortType.DateUpdated);
                break;
            }
            case SortType.DateUpdated:
            {
                /* Genre is not implemented yet
                await ChangeSortType(GroupType.Genre, SortType.Genre);
                break;
            }
            case SortType.Genre:
            {
                */
                await ChangeSortType(GroupType.Folder, SortType.Folder);
                break;
            }
            case SortType.Folder:
            {
                await ChangeSortType(GroupType.Title, SortType.Title);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        Debug.Log($"group by {selectedGroupType}, sort by {selectedSortType}");
    }

    public void OnFavorite()
    {
    }

    public void OnOptions()
    {
        if (page == MenuPage.ExitingMenu) return;

        if (page != MenuPage.ChartPreview)
        {
            // nope sound
            UIAudio.PlaySound(UIAudioController.UISound.Back);
            return;
        }

        UIAudio.PlaySound(UIAudioController.UISound.Impact);
        bgmPreview.FadeoutBgmPreview(true);
        page = MenuPage.ExitingMenu;

        SceneSwitcher.Instance.LoadScene("_Options");
    }

    private async Awaitable LoadAllCards()
    {
        // The loop is async, so SelectedGroupIndex and SelectedEntryIndex can change.
        // However, since all the cards are relative to the initial values, we want to make sure we capture the initial
        // value for the duration of the loop.
        int currentGroupIndex = selectedGroupIndex;
        int currentEntryIndex = selectedEntryIndex;
        List<Awaitable> awaitables = new();

        // When the scene is first loaded, cardOffset should be 0. But if we call LoadAllCards() later, we might have
        // moved around a bit.
        int cardOffset = cardAnimator.CenterCardIndex - cardAnimator.CardHalfCount;

        // Load song data and start loading all the jackets
        for (int i = 0; i < cardAnimator.SongCards.Count; i++)
        {
            SongListEntry entry = songList.RelativeSongEntry(currentGroupIndex, currentEntryIndex,
                i - cardAnimator.CardHalfCount);
            string path = entry.Song.JacketPath;

            int cardIndex = SaturnMath.Modulo(cardOffset + i, cardAnimator.SongCards.Count);

            awaitables.Add(LoadCardJacket(cardIndex, path));
            cardAnimator.SetSongData(cardIndex, FindNearestDifficulty(entry.Difficulties, selectedDifficulty),
                entry.Song);
        }

        // Make sure to await all the jacket loads
        foreach (Awaitable awaitable in awaitables) await awaitable;

        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());
    }

    private async Awaitable LoadCardJacket(int cardIndex, string jacketPath)
    {
        cardAnimator.CurrentJacketPaths[cardIndex] = jacketPath;
        Texture2D jacket = await ImageLoader.LoadImageWebRequest(jacketPath);

        if (cardAnimator.CurrentJacketPaths[cardIndex] != jacketPath)
        {
            // A new jacket has been loaded to this card while we were waiting for this one to load. Abort.
            Destroy(jacket);
            return;
        }

        cardAnimator.SetCardJacket(cardIndex, jacket);

        if (cardAnimator.CenterCardIndex == cardIndex)
            cardAnimator.SetSelectedJacket(jacket);
    }

    private static Difficulty FindNearestDifficulty([NotNull] ICollection<Difficulty> diffs, Difficulty selectedDiff)
    {
        if (diffs.Contains(selectedDiff)) return selectedDiff;

        Difficulty lowerDiff = selectedDiff - 1;
        Difficulty higherDiff = selectedDiff + 1;

        while (lowerDiff >= 0 || higherDiff <= Difficulty.Beyond)
        {
            if (diffs.Contains(lowerDiff)) return lowerDiff;
            if (diffs.Contains(higherDiff)) return higherDiff;

            lowerDiff--;
            higherDiff++;
        }

        return default;
    }

    private static bool HigherDiffExists([NotNull] Dictionary<Difficulty, SongDifficulty> diffs,
        Difficulty selectedDifficulty)
    {
        for (Difficulty i = selectedDifficulty + 1; i <= Difficulty.Beyond; i++)
        {
            if (diffs.ContainsKey(i))
                return true;
        }

        return false;
    }

    private static bool LowerDiffExists([NotNull] Dictionary<Difficulty, SongDifficulty> diffs,
        Difficulty selectedDifficulty)
    {
        for (Difficulty i = selectedDifficulty + 1; i >= 0; i--)
        {
            if (diffs.ContainsKey(i))
                return true;
        }

        return false;
    }


    private void SetBgmValues()
    {
        string path = SelectedDifficultInfo.AudioFilepath;
        float start = SelectedDifficultInfo.PreviewStart;
        float duration = SelectedDifficultInfo.PreviewDuration;
        bgmPreview.SetBgmValues(path, start, duration);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
        if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
        if (Input.GetKeyDown(KeyCode.Q)) OnGroupLeft();
        if (Input.GetKeyDown(KeyCode.E)) OnGroupRight();
        if (Input.GetKeyDown(KeyCode.S)) OnSort();
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
        if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
        if (Input.GetKeyDown(KeyCode.UpArrow)) OnDifficultyChange(+1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) OnDifficultyChange(-1);
        if (Input.GetKeyDown(KeyCode.O)) OnOptions();

        if (Input.GetKeyDown(KeyCode.X)) bgmPreview.FadeoutBgmPreview();
    }
}
}

// ==== Features ====================
// DiffPlus     -    Increase Difficulty
// DiffMinus    -    Decrease Difficulty
// NavLeft      -    Select song on the left
// NavRight     -    Select song on the right
// ToTitle      -    Title Screen
// ToPage0      -    Song Select
// ToPage1      -    Chart Preview
// ToRhythmGame -    Start Gameplay
// ToOptions    -    Options Menu
// Sort         -    Sort Popup + Sort Songs
// Favorite     -    Add song to favorites list
