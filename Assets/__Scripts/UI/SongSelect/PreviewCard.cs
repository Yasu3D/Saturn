using UnityEngine;
using UnityEngine.UI;
using SaturnGame.Data;

namespace SaturnGame.UI
{
    public class PreviewCard : MonoBehaviour
    {
        public RectTransform rect;
        [SerializeField] private Image jacketImage;
        [SerializeField] private Sprite emptyJacket;

        /// <summary>
        /// Set SongData of a card to be empty.
        /// </summary>
        public void SetEmpty()
        {
            jacketImage.sprite = emptyJacket;
        }

        /// <summary>
        /// Set SongData of a Card.
        /// </summary>
        public void SetData(SongData data)
        {
            jacketImage.sprite = data.jacket;
        }
    }
}
