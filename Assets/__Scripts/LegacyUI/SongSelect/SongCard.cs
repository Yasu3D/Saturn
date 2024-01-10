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
        [SerializeField] private SongData songData;

        [Header("Objects")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private RawImage jacketImage;

        [Space(10)]
        [SerializeField] private GameObject cardData;
        [SerializeField] private Texture2D emptyJacket;

        /// <summary>
        /// Set SongData of a card to be empty.
        /// </summary>
        public void SetEmpty()
        {
            jacketImage.texture = emptyJacket;
            cardData.SetActive(false);
        }

        /// <summary>
        /// Set SongData of a Card.
        /// </summary>
        public void SetData(SongData data)
        {
            cardData.SetActive(true);

            titleText.text = data.title;
            artistText.text = data.artist;
            //difficultyText.text = data.GetDifficultyString();
            //jacketImage.texture = data.jacket;
        }
    }
}
