using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class MenuWipeAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform viewMask;
        [SerializeField] private RectTransform textMask;
        [SerializeField] private RectTransform ring1;
        [SerializeField] private RectTransform ring2;
        [SerializeField] private RectTransform ring3;
        private const float maskDuration = 0.65f;
        private const float ring1Duration = 0.12f;
        private const float ring2Duration = 0.12f;
        private const float ring3Duration = 0.12f;
        private readonly Vector2 viewMaskMin = new(0, 0);
        private readonly Vector2 viewMaskMed = new(350, 350);
        private readonly Vector2 viewMaskMax = new(1080, 1080);
        private readonly Vector2 textMaskMin = new(0, 120);
        private readonly Vector2 textMaskMax = new(500, 120);
        private readonly Vector2 ring1Max = new(500, 500);
        private readonly Vector2 ring1Min = new(0, 0);
        private readonly Vector2 ring2Max = new(450, 450);
        private readonly Vector2 ring2Min = new(0, 0);
        private readonly Vector2 ring3Max = new(400, 400);
        private readonly Vector2 ring3Min = new(0, 0);
        private readonly Ease viewEase = Ease.InOutBack;
        private readonly Ease ringEase = Ease.InOutQuad;

        private Sequence currentSequence;

        public void Anim_StartTransition()
        {
            currentSequence.Kill(true);
            currentSequence = DOTween.Sequence();

            viewMask.gameObject.SetActive(true);
            textMask.gameObject.SetActive(true);
            viewMask.sizeDelta = viewMaskMax;
            textMask.sizeDelta = textMaskMin;
            ring1.sizeDelta = ring1Min;
            ring2.sizeDelta = ring2Min;
            ring3.sizeDelta = ring3Min;

            currentSequence.Append(viewMask.DOSizeDelta(viewMaskMed, maskDuration).SetEase(viewEase));
            currentSequence.Insert(0.3f, ring1.DOSizeDelta(ring1Max, ring1Duration).SetEase(ringEase));
            currentSequence.Insert(0.4f, ring2.DOSizeDelta(ring2Max, ring2Duration).SetEase(ringEase));
            currentSequence.Insert(0.45f, ring3.DOSizeDelta(ring3Max, ring3Duration).SetEase(ringEase));

            currentSequence.Insert(0.9f, viewMask.DOSizeDelta(viewMaskMin, 0.12f));
            currentSequence.Insert(0.95f, ring3.DOSizeDelta(ring3Min, 0.12f));
            currentSequence.Insert(1.0f, ring2.DOSizeDelta(ring2Min, 0.12f));
            currentSequence.Insert(1.0f, ring1.DOSizeDelta(ring1Min, 0.12f));
            currentSequence.Insert(1.0f, textMask.DOSizeDelta(textMaskMax, 0.12f).SetEase(Ease.OutQuad));
        }

        public void Anim_EndTransition()
        {
            currentSequence.Kill(true);
            currentSequence = DOTween.Sequence();

            viewMask.sizeDelta = viewMaskMin;
            textMask.sizeDelta = textMaskMax;
            ring1.sizeDelta = ring1Min;
            ring2.sizeDelta = ring2Min;
            ring3.sizeDelta = ring3Min;

            currentSequence.Insert(0.0f, ring1.DOSizeDelta(viewMaskMax, 0.55f).SetEase(ringEase));
            currentSequence.Insert(0.0f, textMask.DOSizeDelta(textMaskMin, 0.12f).SetEase(Ease.OutQuad));
            currentSequence.Insert(0.025f, ring2.DOSizeDelta(viewMaskMax, 0.65f).SetEase(ringEase));
            currentSequence.Insert(0.05f, ring3.DOSizeDelta(viewMaskMax, 0.65f).SetEase(ringEase));
            currentSequence.Insert(0.075f, viewMask.DOSizeDelta(viewMaskMax, 0.65f).SetEase(viewEase)).OnComplete(() =>
                {
                    viewMask.gameObject.SetActive(false);
                    textMask.gameObject.SetActive(false);
                }
            );
        }
    }
}
