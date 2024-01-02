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
        [SerializeField] private Image jacketImage;

        [Space(10)]
        [SerializeField] private GameObject cardData;
        [SerializeField] private Sprite emptyJacket;

        [Header("UI Elements")]
        [SerializeField] private CanvasGroup dataGroup;

        [Header("Animation")]
        [SerializeField] private float animDuration = 0.15f;
        [SerializeField] private Ease animEase = Ease.OutQuad;

        /// <summary>
        /// Hides all content of a card except for the jacket.
        /// </summary>
        public void SetFocus(bool state)
        {
            dataGroup.DOFade(state ? 0 : 1, animDuration).SetEase(animEase);
        }

        /// <summary>
        /// Set SongData of a card to be empty.
        /// </summary>
        public void SetEmpty()
        {
            jacketImage.sprite = emptyJacket;
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
            difficultyText.text = data.GetDifficultyString();
            jacketImage.sprite = data.jacket;
        }
    }
}
