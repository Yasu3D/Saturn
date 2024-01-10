using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class SongMenuManager : MonoBehaviour
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

        private void Anim_ListToPreview()
        {
            listRect.gameObject.SetActive(true);
            previewRect.gameObject.SetActive(true);
            listGroup.alpha = 1;
            listRect.localScale = Vector3.one;
            viewportRect.localScale = Vector3.zero;
            navigatorRect.localScale = Vector3.zero;
            navigatorRect.anchoredPosition = new(navigatorPosA, navigatorRect.anchoredPosition.y);
            panelRect.anchoredPosition = new(panelPosA, panelRect.anchoredPosition.y);
            stripeRect.anchoredPosition = new(stripePosA, stripeRect.anchoredPosition.y);

            listRect.DOScale(listZoomScale, duration).SetEase(Ease.InOutExpo);
            viewportRect.DOScale(1, duration).SetEase(Ease.OutQuad);
            navigatorRect.DOScale(1, duration).SetEase(Ease.OutQuad);
            navigatorRect.DOAnchorPosX(navigatorPosB, 1.5f * duration).SetEase(Ease.OutQuad);
            panelRect.DOAnchorPosX(panelPosB, duration).SetEase(Ease.OutQuad);
            stripeRect.DOAnchorPosX(stripePosB, duration).SetEase(Ease.OutQuad);

            listGroup.DOFade(0, duration).SetEase(Ease.OutExpo).OnComplete(() =>
                listRect.gameObject.SetActive(false)
            );
        }

        private void Anim_PreviewToList()
        {
            listRect.gameObject.SetActive(true);
            previewRect.gameObject.SetActive(true);
            listGroup.alpha = 0;
            listRect.localScale = Vector3.one * listZoomScale;
            viewportRect.localScale = Vector3.one;
            navigatorRect.localScale = Vector3.one;
            navigatorRect.anchoredPosition = new(navigatorPosB, navigatorRect.anchoredPosition.y);
            panelRect.anchoredPosition = new(panelPosB, panelRect.anchoredPosition.y);
            stripeRect.anchoredPosition = new(stripePosB, stripeRect.anchoredPosition.y);

            listGroup.DOFade(1, duration).SetEase(Ease.OutExpo);
            listRect.DOScale(1, duration).SetEase(Ease.OutExpo);
            viewportRect.DOScale(0, duration).SetEase(Ease.OutQuad);
            panelRect.DOAnchorPosX(panelPosA, duration).SetEase(Ease.OutQuad);
            stripeRect.DOAnchorPosX(stripePosA, duration).SetEase(Ease.OutQuad);
            
            navigatorRect.DOScale(0, duration).SetEase(Ease.OutQuad);
            navigatorRect.DOAnchorPosX(navigatorPosA, 1.5f * duration).SetEase(Ease.OutQuad).OnComplete(() =>
                previewRect.gameObject.SetActive(false)
            );
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3)) Anim_ListToPreview();
            if (Input.GetKeyDown(KeyCode.Alpha4)) Anim_PreviewToList();
        }
    }
}