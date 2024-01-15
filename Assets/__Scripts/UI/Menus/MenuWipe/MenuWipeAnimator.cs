using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class MenuWipeAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject group;
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private RectTransform logoRect;
        [SerializeField] private float logoScaleDuration = 0.5f;
        [SerializeField] private Ease logoScaleEase = Ease.OutBack;
        [SerializeField] private float logoSpinDuration = 0.5f;
        [SerializeField] private Ease logoSpinEase = Ease.OutCubic;

        [Space(10)]

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform panelLayer1A;
        [SerializeField] private RectTransform panelLayer2A;
        [SerializeField] private RectTransform panelLayer1B;
        [SerializeField] private RectTransform panelLayer2B;

        [SerializeField] private float panelExtendDuration = 0.5f;
        [SerializeField] private float panelReturnDuration = 0.6f;
        [SerializeField] private Ease panelEase = Ease.OutQuart;

        private bool isActive = false;

        public void StartTransition()
        {
            if (isActive) return;
            isActive = true;

            group.SetActive(true);

            // Logo
            logoRect.localScale = new(5, 5, 1);
            logoRect.eulerAngles = new(0, 0, 180);
            logoGroup.alpha = 1;
            logoRect.DOScale(1, logoScaleDuration).SetEase(logoScaleEase);
            logoRect.DORotate(new Vector3(0,0,0), logoSpinDuration).SetEase(logoSpinEase);

            // Background
            background.sizeDelta = new(0, 1080);
            background.DOSizeDelta(new(1080, 1080), panelExtendDuration).SetEase(panelEase);

            // Panels
            panelLayer1A.anchoredPosition = new(-500, 0);
            panelLayer1B.anchoredPosition = new(-500, 0);
            panelLayer2A.anchoredPosition = new(-375, 0);
            panelLayer2B.anchoredPosition = new(-375, 0);

            panelLayer1A.DOAnchorPosX(-125, panelExtendDuration + 0.1f).SetEase(panelEase);
            panelLayer1B.DOAnchorPosX(-125, panelExtendDuration + 0.1f).SetEase(panelEase);
            panelLayer2A.DOAnchorPosX(-125, panelExtendDuration).SetEase(panelEase);
            panelLayer2B.DOAnchorPosX(-125, panelExtendDuration).SetEase(panelEase);
        }

        public void EndTransition()
        {
            if (!isActive) return;
            isActive = false;

            // Logo
            logoRect.localScale = Vector3.one;
            logoRect.eulerAngles = Vector3.zero;
            logoRect.DOScale(5, logoScaleDuration).SetEase(Ease.InBack);
            logoGroup.DOFade(0, 0.5f).SetEase(Ease.InExpo);

            // Background
            background.sizeDelta = new(1080, 1080);
            background.DOSizeDelta(new(0, 1080), panelExtendDuration).SetEase(Ease.InBack);

            // Panels
            panelLayer1A.anchoredPosition = new(-125, 0);
            panelLayer1B.anchoredPosition = new(-125, 0);
            panelLayer2A.anchoredPosition = new(-125, 0);
            panelLayer2B.anchoredPosition = new(-125, 0);

            panelLayer1A.DOAnchorPosX(-500, panelReturnDuration).SetEase(Ease.InBack);
            panelLayer1B.DOAnchorPosX(-500, panelReturnDuration).SetEase(Ease.InBack);
            panelLayer2A.DOAnchorPosX(-375, panelReturnDuration).SetEase(Ease.InBack);
            panelLayer2B.DOAnchorPosX(-375, panelReturnDuration).SetEase(Ease.InBack).OnComplete(() => group.SetActive(false));
        }
    }
}
