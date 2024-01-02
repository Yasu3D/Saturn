using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NavArrowBounce : MonoBehaviour
{
    [SerializeField] private RectTransform arrowL;
    [SerializeField] private RectTransform arrowR;
    private const float duration = 0.75f;
    private const float destination = 160;
    private Ease ease = Ease.InOutQuad;

    void Awake()
    {
        arrowL.DOAnchorPosX(-destination, duration).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
        arrowR.DOAnchorPosX(destination, duration).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
    }
}
