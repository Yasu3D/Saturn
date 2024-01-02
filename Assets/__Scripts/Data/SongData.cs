using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.Data
{
    public class SongData
    {
        public Sprite jacket;
        public string title;
        public string artist;
        public string charter;
        public float difficulty;

        public string GetDifficultyString()
        {
            return ((int)difficulty).ToString() + (difficulty % 1 > 0.6f ? "+" : "");
        }

        public string GetDifficultyString(float difficulty)
        {
            return ((int)difficulty).ToString() + (difficulty % 1 > 0.6f ? "+" : "");
        }
    }
}
