using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JetBrains.Annotations;
using SaturnGame.Data;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
public class SongSelectCardAnimator : MonoBehaviour
{
    [Header("Cards")]
    [FormerlySerializedAs("songCards")] public List<SongCard> SongCards;

    [SerializeField] private List<PreviewCard> previewCards;

    [Header("Jackets")]
    [SerializeField] private Texture2D emptyJacket;
    [SerializeField] private RawImage selectedJacket0;
    [SerializeField] private RawImage selectedJacket1;
    [SerializeField] private RawImage selectedJacket2;

    [Header("Animation")]
    private const float SongCardScaleA = 0.6f;
    private const float SongCardScaleB = 0.8f;
    private const float TweenDuration = 0.1f;
    private const Ease TweenEase = Ease.OutQuad;

    public string[] CurrentJacketPaths;

    /// <summary>
    /// Index of the card that's currently in the center.
    /// </summary>
    public int CenterCardIndex { get; private set; }

    public int WrapCardIndex { get; private set; }

    /// <summary>
    /// Number of cards on either side of the center card.
    /// </summary>
    public int CardHalfCount => SongCards.Count / 2;

    public enum ShiftDirection
    {
        Left = 1,
        Right = -1,
    }

    private void Awake()
    {
        if (SongCards.Count != previewCards.Count)
            Debug.LogWarning("SongCard and PreviewCard count does not match!");

        CurrentJacketPaths = new string[SongCards.Count];

        CenterCardIndex = CardHalfCount;
    }

    public void Anim_ShiftCards(ShiftDirection direction)
    {
        CenterCardIndex = SaturnMath.Modulo(CenterCardIndex + (int)direction, SongCards.Count);
        WrapCardIndex = SaturnMath.Modulo(CenterCardIndex + CardHalfCount * (int)direction, SongCards.Count);

        for (int i = 0; i < SongCards.Count; i++)
        {
            int animIndex = SaturnMath.Modulo(CardHalfCount - CenterCardIndex + i, SongCards.Count);

            Vector2 cardPos = GetSongCardPosition(animIndex);
            Vector2 previewPos = GetPreviewCardPosition(animIndex);
            float scale = i == CenterCardIndex ? SongCardScaleA : SongCardScaleB;

            if (i == WrapCardIndex)
            {
                SongCards[i].Rect.localScale = Vector3.one * scale;
                SongCards[i].Rect.anchoredPosition = cardPos;
                previewCards[i].Rect.anchoredPosition = previewPos;
            }
            else
            {
                SongCards[i].Rect.DOScale(scale, TweenDuration).SetEase(TweenEase);
                SongCards[i].Rect.DOAnchorPos(cardPos, TweenDuration).SetEase(TweenEase);
                previewCards[i].Rect.DOAnchorPos(previewPos, TweenDuration).SetEase(TweenEase);
            }
        }
    }

    private static Vector2 GetSongCardPosition(int index)
    {
        float[] xPos = { -670, -670, -670, -670, -500, -260, 0, 260, 500, 670, 670, 670, 670 };
        float[] yPos = { -160, -160, -160, -160, -100, -50, -50, -50, -100, -160, -160, -160, -160 };
        return new(xPos[index], yPos[index]);
    }

    private static Vector2 GetPreviewCardPosition(int index)
    {
        float[] xPos = { -700, -472, -384, -296, -208, -120, 0, 120, 208, 296, 384, 472, 700 };
        float[] yPos = { -60, -45, -30, -20, -10, -5, 0, -5, -10, -20, -30, -45, -60 };
        return new(xPos[index], yPos[index]);
    }

    public void SetCardJacket(int cardIndex, Texture2D jacket)
    {
        RawImage jacket0 = SongCards[cardIndex].JacketImage;
        RawImage jacket1 = previewCards[cardIndex].JacketImage;

        // Destroy previous textures to avoid a memory leak.
        if (jacket0.texture && jacket0.texture != emptyJacket) Destroy(jacket0.texture);
        if (jacket1.texture && jacket1.texture != emptyJacket) Destroy(jacket1.texture);

        jacket0.texture = jacket;
        jacket1.texture = jacket;
    }

    public void SetSongData(int cardIndex, int diffIndex, [NotNull] Song data)
    {
        SongCard card = SongCards[cardIndex];
        card.ArtistText.text = data.Artist;
        card.TitleText.text = data.Title;
        card.DifficultyText.text = SaturnMath.GetDifficultyString(data.SongDiffs[diffIndex].Level);
    }

    public void SetSelectedJacket(Texture2D jacket)
    {
        selectedJacket0.texture = jacket;
        selectedJacket1.texture = jacket;
        selectedJacket2.texture = jacket;
    }

    public Texture2D GetCenterCardJacket()
    {
        return (Texture2D)SongCards[CenterCardIndex].JacketImage.texture;
    }
}
}
