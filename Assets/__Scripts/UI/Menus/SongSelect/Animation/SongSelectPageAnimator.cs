using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
public class SongSelectPageAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup listGroup;
    [SerializeField] private RectTransform listRect;
    [SerializeField] private RectTransform previewRect;

    [Space(10)] [SerializeField] private RectTransform navigatorRect;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private RectTransform viewportRect;
    [SerializeField] private RectTransform stripeRect;

    private const float ListZoomScale = 5;
    private const float PanelPosA = 750;
    private const float PanelPosB = 0;
    private const float StripePosA = -750;
    private const float StripePosB = -490;
    private const float NavigatorPosA = 750;
    private const float NavigatorPosB = -300;
    private const float Duration = 0.25f;

    private Sequence currentSequence;

    public void Anim_ToSongSelect()
    {
        currentSequence.Kill(true);
        listRect.gameObject.SetActive(true);
        previewRect.gameObject.SetActive(true);
        listGroup.alpha = 0;
        listRect.localScale = Vector3.one * ListZoomScale;
        viewportRect.localScale = Vector3.one;
        navigatorRect.localScale = Vector3.one;
        navigatorRect.anchoredPosition = new(NavigatorPosB, navigatorRect.anchoredPosition.y);
        panelRect.anchoredPosition = new(PanelPosB, panelRect.anchoredPosition.y);
        stripeRect.anchoredPosition = new(StripePosB, stripeRect.anchoredPosition.y);

        currentSequence = DOTween.Sequence();
        currentSequence.Join(listGroup.DOFade(1, Duration).SetEase(Ease.OutExpo));
        currentSequence.Join(listRect.DOScale(1, Duration).SetEase(Ease.OutExpo));
        currentSequence.Join(viewportRect.DOScale(0, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(panelRect.DOAnchorPosX(PanelPosA, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(stripeRect.DOAnchorPosX(StripePosA, Duration).SetEase(Ease.OutQuad));

        currentSequence.Join(navigatorRect.DOScale(0, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(navigatorRect.DOAnchorPosX(NavigatorPosA, 1.5f * Duration).SetEase(Ease.OutQuad)
            .OnComplete(() => previewRect.gameObject.SetActive(false)));
    }

    public void Anim_ToChartPreview()
    {
        // If you update this, also update ToChartPreviewInstant so the end states match.
        currentSequence.Kill(true);
        listRect.gameObject.SetActive(true);
        previewRect.gameObject.SetActive(true);
        listGroup.alpha = 1;
        listRect.localScale = Vector3.one;
        viewportRect.localScale = Vector3.zero;
        navigatorRect.localScale = Vector3.zero;
        navigatorRect.anchoredPosition = new(NavigatorPosA, navigatorRect.anchoredPosition.y);
        panelRect.anchoredPosition = new(PanelPosA, panelRect.anchoredPosition.y);
        stripeRect.anchoredPosition = new(StripePosA, stripeRect.anchoredPosition.y);

        currentSequence = DOTween.Sequence();
        currentSequence.Join(listRect.DOScale(ListZoomScale, Duration).SetEase(Ease.InOutExpo));
        currentSequence.Join(viewportRect.DOScale(1, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(navigatorRect.DOScale(1, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(navigatorRect.DOAnchorPosX(NavigatorPosB, 1.5f * Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(panelRect.DOAnchorPosX(PanelPosB, Duration).SetEase(Ease.OutQuad));
        currentSequence.Join(stripeRect.DOAnchorPosX(StripePosB, Duration).SetEase(Ease.OutQuad));

        currentSequence.Join(listGroup.DOFade(0, Duration).SetEase(Ease.OutExpo).OnComplete(() =>
            listRect.gameObject.SetActive(false)
        ));
    }

    public void ToChartPreviewInstant()
    {
        // If you update this, also update Anim_ToChartPreview to animate in the end state correctly.
        currentSequence.Kill(true);
        listRect.gameObject.SetActive(true);
        previewRect.gameObject.SetActive(true);
        listGroup.gameObject.SetActive(false);
        listRect.localScale = ListZoomScale * Vector3.one;
        viewportRect.localScale = Vector3.one;
        navigatorRect.localScale = Vector3.one;
        navigatorRect.anchoredPosition = new(NavigatorPosB, navigatorRect.anchoredPosition.y);
        panelRect.anchoredPosition = new(PanelPosB, panelRect.anchoredPosition.y);
        stripeRect.anchoredPosition = new(StripePosB, stripeRect.anchoredPosition.y);
    }
}
}