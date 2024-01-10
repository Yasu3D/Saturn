using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class SongInfoDisplay : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private List<Color> foregroundColors = new List<Color>
        {
            new(0.1019f, 0.4823f, 1.0000f, 1.0000f),
            new(1.0000f, 0.7647f, 0.0000f, 1.0000f),
            new(1.0000f, 0.0000f, 0.5176f, 1.0000f),
            new(0.2509f, 0.0000f, 0.2627f, 1.0000f),
            new(0.0000f, 0.0000f, 0.0000f, 1.0000f)
        };

        [SerializeField] private List<Color> backgroundColors = new List<Color>
        {
            new(0.0823f, 0.2471f, 0.3019f, 1.0000f),
            new(0.3584f, 0.2747f, 0.1775f, 1.0000f),
            new(0.2169f, 0.0051f, 0.1836f, 1.0000f),
            new(0.1212f, 0.0457f, 0.1500f, 1.0000f),
            new(0.0922f, 0.0460f, 0.2169f, 1.0000f)
        };

        [SerializeField] private List<Color> backgroundCheckerColors = new List<Color>
        {
            new(0.0313f, 0.0392f, 0.3215f, 0.3333f),
            new(0.8165f, 0.4488f, 0.0000f, 0.0627f),
            new(0.4941f, 0.0000f, 0.0275f, 0.1000f),
            new(0.0000f, 0.0000f, 0.0000f, 0.5000f),
            new(0.0000f, 0.0000f, 0.0000f, 0.4000f)
        };

        [SerializeField] private List<Image> foregroundImages = new();
        [SerializeField] private List<Image> backgroundImages = new();
        [SerializeField] private List<Image> backgroundCheckerImages = new();
        private const float colorAnimDuration = 0.2f;
        private Ease colorAnimEase = Ease.OutExpo;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private TextMeshProUGUI charterText;
        [SerializeField] private TextMeshProUGUI bpmText;
        [SerializeField] private TextMeshProUGUI difficultyNameText;
        [SerializeField] private TextMeshProUGUI difficultyLevelText;

        public void SetSongInfo(string title, string artist, string charter, float bpm, int diffIndex, float diffLevel)
        {
            SetTitle(title);
            SetArtist(artist);
            SetCharter(charter);
            SetBPM(bpm);
            SetDifficulty(diffIndex, diffLevel);
        }

        public void SetTitle(string title)
        {
            titleText.text = title;
        }

        public void SetArtist(string artist)
        {
            artistText.text = artist;
        }

        public void SetCharter(string charter)
        {
            charterText.text = charter;
        }

        public void SetBPM(float bpm)
        {
            bpmText.text = bpm.ToString();
        }

        public void SetDifficulty(int index, float level)
        {
            string diffName;
            string diffLevel;
            int clampedIndex = Mathf.Clamp(index, 0, 4);

            switch (index)
            {
                case 0:
                    diffName = "NORMAL";
                    break;

                case 1:
                    diffName = "HARD";
                    break;

                case 2:
                    diffName = "EXPERT";
                    break;

                case 3:
                    diffName = "INFERNO";
                    break;

                default:
                    diffName = "BEYOND";
                    break;
            }

            // Convert level to string and add a plus if it's above 0.6
            diffLevel = Mathf.Floor(level).ToString();
            if (level % 1 > 0.6f) diffLevel += "+";

            difficultyNameText.text = diffName;
            difficultyLevelText.text = diffLevel;

            foreach (Image img in foregroundImages)
            {
                img.DOColor(foregroundColors[clampedIndex], colorAnimDuration).SetEase(colorAnimEase);
            }

            foreach (Image img in backgroundImages)
            {
                img.DOColor(backgroundColors[clampedIndex], colorAnimDuration).SetEase(colorAnimEase);
            }

            foreach (Image img in backgroundCheckerImages)
            {
                img.DOColor(backgroundCheckerColors[clampedIndex], colorAnimDuration).SetEase(colorAnimEase);
            }
        }
    }
}
