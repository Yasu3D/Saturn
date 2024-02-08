using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using SaturnGame.Data;

namespace SaturnGame.UI
{
    public class SongSelectCardAnimator : MonoBehaviour
    {

        [Header("Cards")]
        public List<SongCard> songCards;
        [SerializeField] private List<PreviewCard> previewCards;

        [Header("Jackets")]
        [SerializeField] private Texture2D emptyJacket;
        [SerializeField] private RawImage selectedJacket0;
        [SerializeField] private RawImage selectedJacket1;
        [SerializeField] private RawImage selectedJacket2;

        [Header("Animation")]
        private const float songCardScaleA = 0.6f;
        private const float songCardScaleB = 0.8f;
        private const float tweenDuration = 0.1f;
        private readonly Ease tweenEase = Ease.OutQuad;

        /// <summary>
        /// Index of the card that's currently in the center.
        /// </summary>
        public int CenterCardIndex { get; private set; }
        
        public int WrapCardIndex { get; private set; } = 0;

        /// <summary>
        /// Number of cards on either side of the center card.
        /// </summary>
        public int cardHalfCount {get; private set; }

        public enum MoveDirection { Left = 1, Right = -1}

        void Awake()
        {
            if (songCards.Count != previewCards.Count)
                Debug.LogWarning("SongCard and PreviewCard count does not match!");

            cardHalfCount = (int)(songCards.Count * 0.5f);
            CenterCardIndex = cardHalfCount;
        }

        public void Anim_ShiftCards(MoveDirection direction)
        {
            CenterCardIndex = SaturnMath.Modulo(CenterCardIndex + (int)direction, songCards.Count);
            WrapCardIndex = SaturnMath.Modulo(CenterCardIndex + cardHalfCount * (int)direction, songCards.Count);

            for (int i = 0; i < songCards.Count; i++)
            {
                int animIndex = SaturnMath.Modulo(cardHalfCount - CenterCardIndex + i, songCards.Count);

                Vector2 cardPos = GetSongCardPosition(animIndex);
                Vector2 previewPos = GetPreviewCardPosition(animIndex);
                float scale = i == CenterCardIndex ? songCardScaleA : songCardScaleB;

                if (i == WrapCardIndex)
                {
                    songCards[i].rect.localScale = Vector3.one * scale;
                    songCards[i].rect.anchoredPosition = cardPos;
                    previewCards[i].rect.anchoredPosition = previewPos;
                }
                else
                {
                    songCards[i].rect.DOScale(scale, tweenDuration).SetEase(tweenEase);
                    songCards[i].rect.DOAnchorPos(cardPos, tweenDuration).SetEase(tweenEase);
                    previewCards[i].rect.DOAnchorPos(previewPos, tweenDuration).SetEase(tweenEase);
                }
            }
        }

        private Vector2 GetSongCardPosition(int index)
        {
            float[] xPos = {-670, -670, -670, -670, -500, -260,   0, 260,  500,  670,  670,  670,  670};
            float[] yPos = {-160, -160, -160, -160, -100,  -50, -50, -50, -100, -160, -160, -160, -160};
            return new(xPos[index], yPos[index]);
        }

        private Vector2 GetPreviewCardPosition(int index)
        {
            float[] xPos = {-700, -472, -384, -296, -208, -120, 0, 120, 208, 296, 384, 472, 700};
            float[] yPos = { -60,  -45,  -30,  -20,  -10,   -5, 0,  -5, -10, -20, -30, -45, -60};
            return new(xPos[index], yPos[index]);
        }
    
        public void SetCardJacket(int cardIndex, Texture2D jacket)
        {
            RawImage jacket0 = songCards[cardIndex].jacketImage;
            RawImage jacket1 = previewCards[cardIndex].jacketImage;

            // Destroy previous textures to avoid a memory leak.
            if (jacket0.texture && jacket0.texture != emptyJacket) Destroy(jacket0.texture);
            if (jacket1.texture && jacket1.texture != emptyJacket) Destroy(jacket1.texture);

            jacket0.texture = jacket;
            jacket1.texture = jacket;
        }

        public void SetSongData(int cardIndex, int diffIndex, SongData data)
        {
            var card = songCards[cardIndex];
            card.artistText.text = data.artist;
            card.titleText.text = data.title;
            card.difficultyText.text = SaturnMath.GetDifficultyString(data.songDiffs[diffIndex].diffLevel);
        }

        public void SetSelectedJacket(Texture2D jacket)
        {
            selectedJacket0.texture = jacket;
            selectedJacket1.texture = jacket;
            selectedJacket2.texture = jacket;
        }
    
        public Texture2D GetCenterCardJacket()
        {
            return (Texture2D) songCards[CenterCardIndex].jacketImage.texture;
        }
    }
}