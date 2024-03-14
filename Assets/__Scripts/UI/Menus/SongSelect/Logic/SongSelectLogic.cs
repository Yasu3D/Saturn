using System.Threading.Tasks;
using UnityEngine;
using SaturnGame.Loading;
using SaturnGame.Data;
using SaturnGame.RhythmGame;

namespace SaturnGame.UI
{
    public class SongSelectLogic : MonoBehaviour
    {
        public SongSelectCardAnimator cardAnimator;
        public SongSelectPageAnimator pageAnimator;
        public SongSelectDisplayAnimator displayAnimator;
        public SongDatabase songDatabase;
        public ButtonPageManager buttonManager;
        public BgmPreviewController bgmPreview;

        public int SelectedSongIndex { get; private set; } = 0;
        public int SelectedDifficulty { get; private set; } = 0;

        public enum MenuPage
        {
        SongSelect = 0,
        ChartPreview = 1,
        ExitingMenu = 2
        }

        public MenuPage page = MenuPage.SongSelect;

        [SerializeField] private GameObject diffPlusButton0;
        [SerializeField] private GameObject diffPlusButton1;
        [SerializeField] private GameObject diffMinusButton0;
        [SerializeField] private GameObject diffMinusButton1;
        private UIAudioController UIAudio => UIAudioController.Instance;

        void Awake()
        {
            Debug.Log($"Coming from {SceneSwitcher.Instance.LastScene}");
            page = SceneSwitcher.Instance.LastScene == "_Options" ? MenuPage.ChartPreview : MenuPage.SongSelect;

            songDatabase.LoadAllSongData();

            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));


            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);

            // TODO: async
            LoadAllCards();
            SetBgmValues();

            if (page is MenuPage.ChartPreview)
            {
                buttonManager.SetActiveButtons(1);
                pageAnimator.ToChartPreviewInstant();
                string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                LoadChart(chartPath);
            }
        }

        public void OnDifficulutyPlus() 
        {
            if (page == MenuPage.ExitingMenu) return;
            if (SelectedDifficulty >= 4) return;

            int prevDifficulty = SelectedDifficulty;

            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            int index = SelectedDifficulty + 1;
            SelectedDifficulty = FindNearestDifficulty(diffs, index);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);

            SetBgmValues();


            if (prevDifficulty != SelectedDifficulty)
            {
                UIAudio.PlaySound(UIAudioController.UISound.Navigate);

                if (page is MenuPage.ChartPreview)
                {
                    bgmPreview.FadeoutBgmPreview();
                    bgmPreview.ResetLingerTimer();
                    string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                    LoadChart(chartPath);
                }
            }
        }

        public void OnDifficultyMinus() 
        {
            if (page == MenuPage.ExitingMenu) return;
            if (SelectedDifficulty <= 0) return;
            
            int prevDifficulty = SelectedDifficulty;

            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            int index = SelectedDifficulty - 1;
            SelectedDifficulty = FindNearestDifficulty(diffs, index);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            
            SetBgmValues();

            if (prevDifficulty != SelectedDifficulty)
            {
                UIAudio.PlaySound(UIAudioController.UISound.Navigate);
                
                if (page is MenuPage.ChartPreview)
                {
                    bgmPreview.FadeoutBgmPreview();
                    bgmPreview.ResetLingerTimer();

                    string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                    LoadChart(chartPath);
                }
            }
        }
        
        public void OnBack()
        {
            if (page == MenuPage.ExitingMenu) return;
            UIAudio.PlaySound(UIAudioController.UISound.Back);

            if (page is MenuPage.SongSelect)
            {
                page = MenuPage.ExitingMenu;
                bgmPreview.FadeoutBgmPreview(true);
                SceneSwitcher.Instance.LoadScene("_TitleScreen");
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
            if (page == MenuPage.ExitingMenu) return;
            if (page is MenuPage.SongSelect)
            {
                UIAudio.PlaySound(UIAudioController.UISound.Impact);

                bgmPreview.FadeoutBgmPreview();
                bgmPreview.ResetLingerTimer();

                page = MenuPage.ChartPreview;
                pageAnimator.Anim_ToChartPreview();
                buttonManager.SwitchButtons(1);

                string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                LoadChart(chartPath);
                return;
            }

            if (page is MenuPage.ChartPreview)
            {
                UIAudio.PlaySound(UIAudioController.UISound.StartGame);
                bgmPreview.FadeoutBgmPreview(true);
                page = MenuPage.ExitingMenu;

                SceneSwitcher.Instance.LoadScene("_RhythmGame");
            }
        }

        public async void OnNavigateLeft()
        {
            if (page == MenuPage.ExitingMenu) return;
            if (page is not MenuPage.SongSelect) return;

            UIAudio.PlaySound(UIAudioController.UISound.Navigate);

            // Index
            SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex - 1, songDatabase.songs.Count);

            // Diff
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);
            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            // Update Selected Song
            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);

            // Update Cards
            cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Right);
            int newSongIndex = SaturnMath.Modulo(SelectedSongIndex - cardAnimator.cardHalfCount, songDatabase.songs.Count);
            var newSong = songDatabase.songs[newSongIndex];
            Texture2D newJacket = await ImageLoader.LoadImageWebRequest(newSong.jacketPath);

            cardAnimator.SetSongData(cardAnimator.WrapCardIndex, SelectedDifficulty, newSong);
            cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);

            cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

            // Audio Preview
            SetBgmValues();
            bgmPreview.FadeoutBgmPreview();
            bgmPreview.ResetLingerTimer();
        }

        public async void OnNavigateRight()
        {
            if (page == MenuPage.ExitingMenu) return;
            if (page is not MenuPage.SongSelect) return;

            UIAudio.PlaySound(UIAudioController.UISound.Navigate);

            // Index
            SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex + 1, songDatabase.songs.Count);

            // Diffs
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);
            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            // Update Selected Song
            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            
            // Update Cards
            cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Left);

            int newSongIndex = SaturnMath.Modulo(SelectedSongIndex + cardAnimator.cardHalfCount, songDatabase.songs.Count);
            var newSong = songDatabase.songs[newSongIndex];
            Texture2D newJacket = await ImageLoader.LoadImageWebRequest(newSong.jacketPath);
            
            cardAnimator.SetSongData(cardAnimator.WrapCardIndex, SelectedDifficulty, newSong);
            cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);

            cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

            // Audio Preview
            SetBgmValues();
            bgmPreview.FadeoutBgmPreview();
            bgmPreview.ResetLingerTimer();
        }

        public void OnSort() {}
        public void OnFavorite() {}

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



        private async void LoadAllCards()
        {
            for (int i = 0; i < cardAnimator.songCards.Count; i++)
            {
                int index = SaturnMath.Modulo(i - cardAnimator.cardHalfCount, songDatabase.songs.Count);
                SongData data = songDatabase.songs[index];
                string path = data.jacketPath;

                Texture2D jacket = await ImageLoader.LoadImageWebRequest(path);
                cardAnimator.SetCardJacket(i, jacket);
                cardAnimator.SetSongData(i, SelectedDifficulty, data);
            }

            cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());
        }

        private int FindNearestDifficulty(SongDifficulty[] diffs, int selectedIndex)
        {
            if (diffs[selectedIndex].exists) return selectedIndex;

            int leftIndex = selectedIndex - 1;
            int rightIndex = selectedIndex + 1;

            while (leftIndex >= 0 || rightIndex < diffs.Length)
            {
                
                if (leftIndex >= 0 && diffs[leftIndex].exists) return leftIndex;
                if (rightIndex < diffs.Length && diffs[rightIndex].exists) return rightIndex;

                leftIndex--;
                rightIndex++;
            }

            return 0;
        }

        private bool HigherDiffExists(SongDifficulty[] diffs, int selectedIndex)
        {
            for (int i = selectedIndex + 1; i < diffs.Length; i++)
            {
                if (diffs[i].exists) return true;
            }

            return false;
        }

        private bool LowerDiffExists(SongDifficulty[] diffs, int selectedIndex)
        {
            for (int i = selectedIndex - 1; i >= 0; i--)
            {
                if (diffs[i].exists) return true;
            }

            return false;
        }



        private void SetBgmValues()
        {
            string path = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].audioFilepath;
            float start = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].previewStart;
            float duration = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].previewDuration;
            bgmPreview.SetBgmValues(path, start, duration);
        }

        private async void LoadChart(string path)
        {
            await ChartManager.Instance.LoadChart(path);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
            if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
            if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
            if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
            if (Input.GetKeyDown(KeyCode.UpArrow)) OnDifficulutyPlus();
            if (Input.GetKeyDown(KeyCode.DownArrow)) OnDifficultyMinus();
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
