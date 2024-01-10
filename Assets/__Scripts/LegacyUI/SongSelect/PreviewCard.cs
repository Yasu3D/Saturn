using UnityEngine;
using UnityEngine.UI;
using SaturnGame.Data;

namespace SaturnGame.UI
{
    public class PreviewCard : MonoBehaviour
    {
        public RectTransform rect;
        [SerializeField] private RawImage jacketImage;
        [SerializeField] private Texture2D emptyJacket;

        /// <summary>
        /// Set SongData of a card to be empty.
        /// </summary>
        public void SetEmpty()
        {
            jacketImage.texture = emptyJacket;
        }

        /// <summary>
        /// Set SongData of a Card.
        /// </summary>
        public void SetData(SongData data)
        {
            jacketImage.texture = data.jacket;
        }
    }
}
