using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class SongSelectPageAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup listGroup;
        [SerializeField] private RectTransform listRect;
        [SerializeField] private RectTransform previewRect;

        [Space(10)]
        [SerializeField] private RectTransform navigatorRect;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private RectTransform stripeRect;

        private const float listZoomScale = 5;
        private const float panelPosA = 750;
        private const float panelPosB = 0;
        private const float stripePosA = -750;
        private const float stripePosB = -490;
        private const float navigatorPosA = 750;
        private const float navigatorPosB = -300;
        private const float duration = 0.25f;

        private Sequence currentSequence;

        public void Anim_ToSongSelect()
        {
            currentSequence.Kill(true);
            listRect.gameObject.SetActive(true);
            previewRect.gameObject.SetActive(true);
            listGroup.alpha = 0;
            listRect.localScale = Vector3.one * listZoomScale;
            viewportRect.localScale = Vector3.one;
            navigatorRect.localScale = Vector3.one;
            navigatorRect.anchoredPosition = new(navigatorPosB, navigatorRect.anchoredPosition.y);
            panelRect.anchoredPosition = new(panelPosB, panelRect.anchoredPosition.y);
            stripeRect.anchoredPosition = new(stripePosB, stripeRect.anchoredPosition.y);

            currentSequence = DOTween.Sequence();
            currentSequence.Append(listGroup.DOFade(1, duration).SetEase(Ease.OutExpo));
            currentSequence.Join(listRect.DOScale(1, duration).SetEase(Ease.OutExpo));
            currentSequence.Join(viewportRect.DOScale(0, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(panelRect.DOAnchorPosX(panelPosA, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(stripeRect.DOAnchorPosX(stripePosA, duration).SetEase(Ease.OutQuad));
            
            currentSequence.Join(navigatorRect.DOScale(0, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(navigatorRect.DOAnchorPosX(navigatorPosA, 1.5f * duration).SetEase(Ease.OutQuad).OnComplete(() =>
                previewRect.gameObject.SetActive(false)
            ));
        }

        public void Anim_ToChartPreview()
        {
            currentSequence.Kill(true);
            listRect.gameObject.SetActive(true);
            previewRect.gameObject.SetActive(true);
            listGroup.alpha = 1;
            listRect.localScale = Vector3.one;
            viewportRect.localScale = Vector3.zero;
            navigatorRect.localScale = Vector3.zero;
            navigatorRect.anchoredPosition = new(navigatorPosA, navigatorRect.anchoredPosition.y);
            panelRect.anchoredPosition = new(panelPosA, panelRect.anchoredPosition.y);
            stripeRect.anchoredPosition = new(stripePosA, stripeRect.anchoredPosition.y);

            currentSequence = DOTween.Sequence();
            currentSequence.Append(listRect.DOScale(listZoomScale, duration).SetEase(Ease.InOutExpo));
            currentSequence.Join(viewportRect.DOScale(1, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(navigatorRect.DOScale(1, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(navigatorRect.DOAnchorPosX(navigatorPosB, 1.5f * duration).SetEase(Ease.OutQuad));
            currentSequence.Join(panelRect.DOAnchorPosX(panelPosB, duration).SetEase(Ease.OutQuad));
            currentSequence.Join(stripeRect.DOAnchorPosX(stripePosB, duration).SetEase(Ease.OutQuad));

            currentSequence.Join(listGroup.DOFade(0, duration).SetEase(Ease.OutExpo).OnComplete(() =>
                listRect.gameObject.SetActive(false)
            ));
        }
    }
}