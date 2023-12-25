using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
    public class SongInfoDisplay : MonoBehaviour
    {
        [SerializeField] private string testTitle;
        [SerializeField] private string testArtist;
        [SerializeField] private string testCharter;
        [SerializeField] private float testBpm;
        [SerializeField] private int testIndex = 0;
        [SerializeField] private float testLevel = 1;
        void Update()
        {
            //SetSongInfo(testTitle, testArtist, testCharter, testBpm, testIndex, testLevel);
        }

        [Header("Colors")]
        [SerializeField] private Color normalColor = new(0.1019f, 0.4823f, 1f, 1f);
        [SerializeField] private Color hardColor = new(1f, 0.7647f, 0f, 1f);
        [SerializeField] private Color expertColor = new(1f, 0f, 0.5176f, 1f);
        [SerializeField] private Color infernoColor = new(0.2509f, 0f, 0.2627f, 1f);
        [SerializeField] private Color beyondColor = new(0f, 0f, 0f, 1f);
        [SerializeField] private List<Image> coloredImages = new();
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
            Color color;
            switch (index)
            {
                case 0:
                    diffName = "NORMAL";
                    color = normalColor;
                    break;

                case 1:
                    diffName = "HARD";
                    color = hardColor;
                    break;

                case 2:
                    diffName = "EXPERT";
                    color = expertColor;
                    break;

                case 3:
                    diffName = "INFERNO";
                    color = infernoColor;
                    break;

                default:
                    diffName = "BEYOND";
                    color = beyondColor;
                    break;
            }

            // Convert level to string and add a plus if it's above 0.6
            diffLevel = Mathf.Floor(level).ToString();
            if (level % 1 > 0.6f) diffLevel += "+";

            difficultyNameText.text = diffName;
            difficultyLevelText.text = diffLevel;

            foreach (Image img in coloredImages)
            {
                img.color = color;
            }
        }
    }
}
