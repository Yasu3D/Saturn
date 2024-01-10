using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using SaturnGame.Loading;

namespace SaturnGame.UI
{
    public class SongCardManager : MonoBehaviour
    {
        public int selectedSongIndex = 0;
        [SerializeField] private SongInfoDisplay infoDisplay;
        [SerializeField] private SongLoader songLoader;
        
        [Header("Cards")]
        [SerializeField] private List<SongCard> songCards;
        [SerializeField] private List<PreviewCard> previewCards;

        [Header("Animation")]
        private const float songCardPos = -56;
        private const float songCardScaleA = 0.6f;
        private const float songCardScaleB = 0.8f;
        private const float songCardOffsetA = 290;
        private const float songCardOffsetB = 250;
        private const float previewCardOffset = 100;
        private const float tweenDuration = 0.1f;
        private Ease tweenEase = Ease.OutQuad;

        /// <summary>
        /// Index of the card that's currently in the center.
        /// </summary>
        private int centerCardIndex;
        /// <summary>
        /// Number of cards on either side of the center card.
        /// </summary>
        private int cardHalfCount;
        public enum MoveDirection { Left = 1, Right = -1}

        void Awake()
        {
            if (songCards.Count != previewCards.Count)
                Debug.LogWarning("SongCard and PreviewCard count does not match!");

            cardHalfCount = (int)(songCards.Count * 0.5f);
            centerCardIndex = cardHalfCount;
        }

        // ==== DEBUG ONLY!!! DELETE PLS!!!
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) MoveCards(MoveDirection.Left);
            if (Input.GetKeyDown(KeyCode.D)) MoveCards(MoveDirection.Right);
        }
        // =================================

        /// <summary>
        /// Moves the SongCards left or right.
        /// </summary>
        public void MoveCards(MoveDirection direction)
        {
            selectedSongIndex += (int)direction;
            centerCardIndex = SaturnMath.Modulo(centerCardIndex + (int)direction, songCards.Count);
            int wrapCardIndex = SaturnMath.Loop(centerCardIndex + cardHalfCount * (int)direction, 0, songCards.Count - 1);

            for (int i = 0; i < songCards.Count; i++)
            {
                bool isCenter = i == centerCardIndex;
                bool wrap = i == wrapCardIndex;

                float x1 = GetNewSongCardPosition(i, cardHalfCount, isCenter);
                float y1 = songCardPos;

                float x2 = GetNewPreviewCardPosition(i, cardHalfCount, isCenter);
                float y2 = previewCards[i].rect.anchoredPosition.y;

                float scale = isCenter ? songCardScaleA : songCardScaleB;

                if (wrap)
                {
                    songCards[i].rect.localScale = Vector3.one * scale;
                    songCards[i].rect.anchoredPosition = new(x1, y1);
                    previewCards[i].rect.anchoredPosition = new(x2, y2);
                }
                else
                {
                    songCards[i].rect.DOScale(scale, tweenDuration).SetEase(tweenEase);
                    songCards[i].rect.DOAnchorPos(new(x1, y1), tweenDuration).SetEase(tweenEase);
                    previewCards[i].rect.DOAnchorPos(new(x2, y2), tweenDuration).SetEase(tweenEase);
                }
            }
        }

        private float GetNewSongCardPosition(int index, int halfCount, bool isCenter)
        {
            if (isCenter) return 0;
            
            int distance = SaturnMath.Loop(centerCardIndex - index, -halfCount, halfCount);
            float direction = Mathf.Sign(distance);
            float multiplier = (Mathf.Abs(distance) - 1) * Mathf.Sign(distance);
        
            return songCardOffsetA * direction + songCardOffsetB * multiplier;
        }

        private float GetNewPreviewCardPosition(int index, int halfCount, bool isCenter)
        {
            if (isCenter) return 0;

            int multiplier = SaturnMath.Loop(centerCardIndex - index, -halfCount, halfCount);
            return previewCardOffset * multiplier;
        }
    
        private void SetSongData(int cardIndex)
        {
            int relativeIndex = 0;

            SongCard card = songCards[cardIndex];
            card.SetData(songLoader.songs[selectedSongIndex]);
        }
    }
}