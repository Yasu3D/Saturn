using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SongMenuManager : MonoBehaviour
{
    [Header("Groups")]
    [SerializeField] private CanvasGroup songSelectGroup;
    [SerializeField] private CanvasGroup chartPreviewGroup;

    [Header("Rects")]
    [SerializeField] private RectTransform songSelectRect;
    [SerializeField] private RectTransform chartPreviewRect;

    // ChartPreview Objects
    [SerializeField] private RectTransform navigatorRect;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform viewportRect;

    [Header("Animation")]
    [SerializeField] private ButtonSwitcher buttonSwitcher;
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private Ease tweenEaseSharp = Ease.InOutExpo;
    [SerializeField] private Ease tweenEaseNormal = Ease.OutQuad;
    private const float songSelectScale = 5;
    private const float panelRectPosA = 750;
    private const float panelRectPosB = 0;
    private const float navigatorRectPosA = 750;
    private const float navigatorRectPosB = -300;
    

        // ==== DEBUG ONLY!!! DELETE PLS!!!
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3)) ShowChartPreview();
            if (Input.GetKeyDown(KeyCode.Alpha4)) ShowSongSelect();
        }
        // =================================

    public void ShowChartPreview()
    {
        // Prepare for animation
        songSelectGroup.alpha = 1;
        songSelectRect.localScale = Vector3.one;
        viewportRect.localScale = new(0, 0, 1);
        songSelectGroup.gameObject.SetActive(true);
        chartPreviewGroup.gameObject.SetActive(false);
        panelRect.anchoredPosition = new(panelRectPosA, 0);
        navigatorRect.anchoredPosition = new(navigatorRectPosA, -50);

        // Animate
        songSelectRect.DOScale(songSelectScale, tweenDuration).SetEase(tweenEaseSharp);
        songSelectGroup.DOFade(0, tweenDuration).SetEase(Ease.OutExpo).OnComplete(() => songSelectGroup.gameObject.SetActive(false));
        chartPreviewGroup.gameObject.SetActive(true);
        panelRect.DOAnchorPosX(panelRectPosB, 0.25f).SetEase(tweenEaseNormal);
        viewportRect.DOScale(1, 0.25f).SetEase(tweenEaseNormal);
        navigatorRect.DOAnchorPosX(navigatorRectPosB, 1.25f * tweenDuration).SetEase(tweenEaseNormal);

        buttonSwitcher.SwitchButtons(1);
    }

    public void ShowSongSelect()
    {
        // Prepare for animation
        songSelectGroup.alpha = 0;
        songSelectRect.localScale = Vector3.one * songSelectScale;
        songSelectGroup.gameObject.SetActive(true);
        chartPreviewGroup.gameObject.SetActive(true);
        panelRect.anchoredPosition = new(panelRectPosB, panelRect.anchoredPosition.y);
        viewportRect.localScale = Vector3.one;
        navigatorRect.anchoredPosition = new(navigatorRectPosB, navigatorRect.anchoredPosition.y);

        // Animate
        songSelectRect.DOScale(1, tweenDuration).SetEase(Ease.OutExpo);
        songSelectGroup.DOFade(1, tweenDuration).SetEase(Ease.OutExpo);
        panelRect.DOAnchorPosX(panelRectPosA, 0.25f).SetEase(tweenEaseNormal);
        viewportRect.DOScale(0, 0.25f).SetEase(tweenEaseNormal);
        navigatorRect.DOAnchorPosX(navigatorRectPosA, 1.25f * tweenDuration).SetEase(tweenEaseNormal).OnComplete(() => chartPreviewGroup.gameObject.SetActive(false));

        buttonSwitcher.SwitchButtons(0);
    }
}
