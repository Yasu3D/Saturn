using UnityEngine;
using SaturnGame.UI;
using SaturnGame.Loading;
using SaturnGame.Data;
using SaturnGame;

public class SongSelectLogic : MonoBehaviour
{
    public SongSelectCardAnimator cardAnimator;
    public SongSelectPageAnimator pageAnimator;
    public SongSelectDisplayAnimator displayAnimator;
    public SongDatabase songList;
    public ButtonPageManager buttonManager;
    public int SelectedSongIndex { get; private set; } = 0;
    public int SelectedDifficulty { get; private set; } = 0;

    public enum MenuPage { SongSelect = 0, ChartPreview = 1 }
    public MenuPage page = MenuPage.SongSelect;

    void Awake()
    {
        songList.LoadAllSongData();
        displayAnimator.SetSongData(songList.songs[SelectedSongIndex], SelectedDifficulty);
        LoadAllCardJackets();
    }

    public void OnDifficulutyPlus() 
    {
        // Handle any necessary clamping
        // and limiting depending on how
        // many difficulties a chart has.

        if (SelectedDifficulty >= 4) return;
        SelectedDifficulty ++;
        displayAnimator.SetSongData(songList.songs[SelectedSongIndex], SelectedDifficulty);
    }

    public void OnDifficultyMinus() 
    {
        // Handle any necessary clamping
        // and limiting depending on how
        // many difficulties a chart has.

        if (SelectedDifficulty <= 0) return;
        SelectedDifficulty --;
        displayAnimator.SetSongData(songList.songs[SelectedSongIndex], SelectedDifficulty);
    }
    
    public void OnBack()
    {
        if (page is MenuPage.SongSelect)
        {
            // Go to Main Menu here
            return;
        }

        if (page is MenuPage.ChartPreview)
        {
            page = MenuPage.SongSelect;
            pageAnimator.Anim_ToSongSelect();
            buttonManager.SwitchButtons(0);
            return;
        }
    }

    public void OnConfirm()
    {
        if (page is MenuPage.SongSelect)
        {
            page = MenuPage.ChartPreview;
            pageAnimator.Anim_ToChartPreview();
            buttonManager.SwitchButtons(1);
            return;
        }

        if (page is MenuPage.ChartPreview)
        {
            // Go to rhythm game here
            return;
        }
    }

    public async void OnNavigateLeft()
    {
        if (page is not MenuPage.SongSelect) return;

        // Selected Song Index - 1

        // Set Song Data to new selected song
        // Move Cards to the right by 1
        // Set Preview Jackets to new selected song

        // Unload Jacket of wrapping card (rightmost card)
        // Load new Jacket for wrapping card (rightmost card)

        SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex - 1, songList.songs.Count);

        displayAnimator.SetSongData(songList.songs[SelectedSongIndex], SelectedDifficulty);
        cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Right);
        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

        int newSongIndex = SaturnMath.Modulo(SelectedSongIndex - cardAnimator.cardHalfCount, songList.songs.Count);
        Texture2D newJacket = await ImageLoader.LoadImageWebRequest(songList.songs[newSongIndex].jacketPath);
        cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);
    }

    public async void OnNavigateRight()
    {
        if (page is not MenuPage.SongSelect) return;

        // Selected Song Index + 1

        // Set Song Data to new selected song
        // Move Cards to the left by 1
        // Set Preview Jackets to new selected song

        // Unload Jacket of wrapping card (leftmost card)
        // Load new Jacket for wrapping card (leftmost card)


        SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex + 1, songList.songs.Count);

        displayAnimator.SetSongData(songList.songs[SelectedSongIndex], SelectedDifficulty);
        cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Left);
        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

        int newSongIndex = SaturnMath.Modulo(SelectedSongIndex + cardAnimator.cardHalfCount, songList.songs.Count);
        Texture2D newJacket = await ImageLoader.LoadImageWebRequest(songList.songs[newSongIndex].jacketPath);
        cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);
    }

    public void OnSort() {}
    public void OnFavorite() {}
    public void OnOptions() {}

    private async void LoadAllCardJackets()
    {
        for (int i = 0; i < cardAnimator.songCards.Count; i++)
        {
            int index = SaturnMath.Modulo(i - cardAnimator.cardHalfCount, songList.songs.Count);
            string path = songList.songs[index].jacketPath;
            Texture2D jacket = await ImageLoader.LoadImageWebRequest(path);
            cardAnimator.SetCardJacket(i, jacket);
        }

        cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
        if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
        if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
        if (Input.GetKeyDown(KeyCode.UpArrow)) OnDifficulutyPlus();
        if (Input.GetKeyDown(KeyCode.DownArrow)) OnDifficultyMinus();
    }
}
