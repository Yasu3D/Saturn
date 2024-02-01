using System.Threading.Tasks;
using UnityEngine;
using SaturnGame.Loading;
using SaturnGame.Data;
using SaturnGame.RhythmGame;
using System.Threading;

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

        public enum MenuPage { SongSelect = 0, ChartPreview = 1 }
        public MenuPage page = MenuPage.SongSelect;

        [SerializeField] private GameObject diffPlusButton0;
        [SerializeField] private GameObject diffPlusButton1;
        [SerializeField] private GameObject diffMinusButton0;
        [SerializeField] private GameObject diffMinusButton1;

        void Awake()
        {
            songDatabase.LoadAllSongData();
            
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));


            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            LoadAllCardJackets();
            SetBgmValues();
        }

        public void OnDifficulutyPlus() 
        {
            if (SelectedDifficulty >= 4) return;

            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            int index = SelectedDifficulty + 1;
            SelectedDifficulty = FindNearestDifficulty(diffs, index);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);

            SetBgmValues();

            if (page is MenuPage.ChartPreview)
            {
                string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                LoadChart(chartPath);
            }
        }

        public void OnDifficultyMinus() 
        {
            if (SelectedDifficulty <= 0) return;
            
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            int index = SelectedDifficulty - 1;
            SelectedDifficulty = FindNearestDifficulty(diffs, index);

            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            
            SetBgmValues();

            if (page is MenuPage.ChartPreview)
            {
                string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                LoadChart(chartPath);
            }
        }
        
        public void OnBack()
        {
            if (page is MenuPage.SongSelect)
            {
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
            if (page is MenuPage.SongSelect)
            {
                page = MenuPage.ChartPreview;
                pageAnimator.Anim_ToChartPreview();
                buttonManager.SwitchButtons(1);

                string chartPath = songDatabase.songs[SelectedSongIndex].songDiffs[SelectedDifficulty].chartFilepath;
                //LoadChart(chartPath);
                return;
            }

            if (page is MenuPage.ChartPreview)
            {
                SceneSwitcher.Instance.LoadScene("_RhythmGame");
                return;
            }
        }

        public async void OnNavigateLeft()
        {
            if (page is not MenuPage.SongSelect) return;

            // Index
            SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex - 1, songDatabase.songs.Count);

            // Diff
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);
            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            // SongData and Cards
            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Right);
            cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

            // Jacket Loading
            int newSongIndex = SaturnMath.Modulo(SelectedSongIndex - cardAnimator.cardHalfCount, songDatabase.songs.Count);
            Texture2D newJacket = await ImageLoader.LoadImageWebRequest(songDatabase.songs[newSongIndex].jacketPath);
            cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);

            // Audio Preview
            SetBgmValues();
            bgmPreview.StopBgmPreview();
            bgmPreview.ResetLingerTimer();
        }

        public async void OnNavigateRight()
        {
            if (page is not MenuPage.SongSelect) return;

            // Index
            SelectedSongIndex = SaturnMath.Modulo(SelectedSongIndex + 1, songDatabase.songs.Count);

            // Diffs
            SongDifficulty[] diffs = songDatabase.songs[SelectedSongIndex].songDiffs;
            SelectedDifficulty = FindNearestDifficulty(diffs, SelectedDifficulty);
            diffPlusButton0.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffPlusButton1.SetActive(HigherDiffExists(diffs, SelectedDifficulty));
            diffMinusButton0.SetActive(LowerDiffExists(diffs, SelectedDifficulty));
            diffMinusButton1.SetActive(LowerDiffExists(diffs, SelectedDifficulty));

            // SongData and Cards
            displayAnimator.SetSongData(songDatabase.songs[SelectedSongIndex], SelectedDifficulty);
            cardAnimator.Anim_ShiftCards(SongSelectCardAnimator.MoveDirection.Left);
            cardAnimator.SetSelectedJacket(cardAnimator.GetCenterCardJacket());

            // Jacket
            int newSongIndex = SaturnMath.Modulo(SelectedSongIndex + cardAnimator.cardHalfCount, songDatabase.songs.Count);
            Texture2D newJacket = await ImageLoader.LoadImageWebRequest(songDatabase.songs[newSongIndex].jacketPath);
            cardAnimator.SetCardJacket(cardAnimator.WrapCardIndex, newJacket);

            // Audio Preview
            SetBgmValues();
            bgmPreview.StopBgmPreview();
            bgmPreview.ResetLingerTimer();
        }

        public void OnSort() {}
        public void OnFavorite() {}
        public void OnOptions() {}



        private async void LoadAllCardJackets()
        {
            for (int i = 0; i < cardAnimator.songCards.Count; i++)
            {
                int index = SaturnMath.Modulo(i - cardAnimator.cardHalfCount, songDatabase.songs.Count);
                string path = songDatabase.songs[index].jacketPath;
                Texture2D jacket = await ImageLoader.LoadImageWebRequest(path);
                cardAnimator.SetCardJacket(i, jacket);
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
