using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using SaturnGame.Data;

namespace SaturnGame.UI
{
    public class SongCard : MonoBehaviour
    {
        public RectTransform rect;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI artistText;
        public TextMeshProUGUI difficultyText;
        public RawImage jacketImage;
    }
}
