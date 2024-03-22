using DG.Tweening;
using SaturnGame.Settings;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame.UI
{
    public class OptionsPanelAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject linearPanelGroup;
        [SerializeField] private GameObject radialPanelGroup;

        [SerializeField] private List<OptionPanelLinear> linearPanels;
        [SerializeField] private List<OptionPanelRadial> radialPanels;

        [SerializeField] private OptionPanelPrimary primaryPanel;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private RectTransform panelGroupRect;
        [SerializeField] private RectTransform gradientRect;
        [SerializeField] private RectTransform spinnyThingRect;
        [SerializeField] private RectTransform navigatorRect;
        [SerializeField] private RectMask2D panelMask;
        [SerializeField] private Image glassImage;
        [SerializeField] private Image radialCenterImage;
        [SerializeField] private GameObject radialCoverRing;
        [SerializeField] private GameObject radialCoverBackground;

        private Sequence currentSequence;

        private int LinearCenterIndex { get; set; }
        private int LinearWrapIndex { get; set; }
        private int RadialCenterIndex { get; set; }
        private int RadialWrapIndex { get; set; }
        private int LinearHalfCount => (int)(linearPanels.Count * 0.5f);
        private int RadialHalfCount => (int)(radialPanels.Count * 0.5f);

        public enum MoveDirection { Up = -1, Down = 1 }

        public void Anim_ShiftPanels(MoveDirection direction, int currentIndex, [NotNull] UIScreen screen)
        {
            const float time = 0.05f;
            const Ease ease = Ease.Linear;

            if (screen.screenType is UIScreen.UIScreenType.Radial) animateRadial();
            else animateLinear();
            return;

            void animateLinear()
            {
                LinearCenterIndex = SaturnMath.Modulo(LinearCenterIndex + (int)direction, linearPanels.Count);
                LinearWrapIndex = SaturnMath.Modulo(LinearCenterIndex + LinearHalfCount * (int)direction,
                    linearPanels.Count);

                for (int i = 0; i < linearPanels.Count; i++)
                {
                    OptionPanelLinear panel = linearPanels[i];
                    int index = SaturnMath.Modulo(LinearHalfCount - LinearCenterIndex + i, linearPanels.Count);
                    bool wrap = i == LinearWrapIndex;

                    Vector2 position = GetLinearPosition(index);
                    float scale = GetLinearScale(index);
                    float duration = wrap ? 0 : time;

                    panel.rect.DOAnchorPos(position, duration).SetEase(ease);
                    panel.rect.DOScale(scale, duration).SetEase(ease);

                    if (!wrap) continue;

                    int itemIndex = currentIndex + LinearHalfCount * (int)direction;
                    bool active = itemIndex >= 0 && itemIndex < screen.listItems.Count;
                    panel.gameObject.SetActive(active);
                    if (active) SetPanelLinear(screen, screen.listItems[itemIndex], panel);
                }
            }

            void animateRadial()
            {
                RadialCenterIndex = SaturnMath.Modulo(RadialCenterIndex + (int)direction, radialPanels.Count);
                RadialWrapIndex = SaturnMath.Modulo(RadialCenterIndex + RadialHalfCount * (int)direction,
                    radialPanels.Count);

                for (int i = 0; i < radialPanels.Count; i++)
                {
                    OptionPanelRadial panel = radialPanels[i];
                    int index = SaturnMath.Modulo(RadialHalfCount - RadialCenterIndex + i, radialPanels.Count);
                    bool wrap = i == RadialWrapIndex;

                    Vector3 angle = GetRadialAngle(index);
                    float duration = wrap ? 0 : time;

                    panel.rect.DORotate(angle, duration).SetEase(ease);

                    if (!wrap) continue;

                    int itemIndex = currentIndex + RadialHalfCount * (int)direction;
                    bool active = itemIndex >= 0 && itemIndex < screen.listItems.Count;
                    panel.gameObject.SetActive(active);
                    if (active) SetPanelRadial(screen.listItems[itemIndex], panel);
                }
            }
        }

        public void Anim_ShowPanels([NotNull] UIScreen previous, [NotNull] UIScreen next)
        {
            bool prevLinear =
                previous.screenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;
            bool nextLinear =
                next.screenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;

            if (!nextLinear)
            {
                Anim_ShowPanelsRadial();
                return;
            }

            if (prevLinear)
                Anim_ShowPanelsLinearPartial();
            else
                Anim_ShowPanelsLinearFull();

            // Linear -> Linear => Partial
            // Linear -> Radial => Radial
            // Radial -> Linear => Full
            // Radial -> Radial => Radial
        }

        public void Anim_HidePanels([NotNull] UIScreen previous, [NotNull] UIScreen next)
        {
            bool prevLinear =
                previous.screenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;
            bool nextLinear =
                next.screenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;

            if (!prevLinear)
            {
                Anim_HidePanelsRadial();
                return;
            }

            if (nextLinear)
                Anim_HidePanelsLinearPartial();
            else
                Anim_HidePanelsLinearFull();

            // Linear -> Linear => Partial
            // Linear -> Radial => Full
            // Radial -> Linear => Radial
            // Radial -> Radial => Radial
        }


        private void Anim_ShowPanelsLinearPartial()
        {
            // 1 frame = 32ms
            // Move panels 6 frames OutQuad
            // Fade Panels 6 frames OutQuad

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = true;
            radialCenterImage.gameObject.SetActive(false);
            radialCoverRing.SetActive(false);
            radialCoverBackground.SetActive(false);

            panelGroup.alpha = 0;
            panelGroupRect.anchoredPosition = new Vector2(-250, 0);
            panelGroupRect.eulerAngles = new Vector3(0, 0, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(1, frame * 6).SetEase(Ease.OutQuad));
            currentSequence.Join(panelGroupRect.DOAnchorPosX(0, frame * 6).SetEase(Ease.OutQuad));
        }

        private void Anim_HidePanelsLinearPartial()
        {
            // 1 frame = 32ms
            // Move panels 4 frames InQuad

            // wait 2 frames
            // Fade panels out 2 frames Linear

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = true;
            radialCenterImage.gameObject.SetActive(false);
            radialCoverRing.SetActive(false);
            radialCoverBackground.SetActive(false);

            panelGroupRect.anchoredPosition = new Vector2(0, 0);
            panelGroupRect.eulerAngles = new Vector3(0, 0, 0);
            panelGroup.alpha = 1;

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroupRect.DOAnchorPosX(-250, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Insert(frame * 2, panelGroup.DOFade(0, frame * 2).SetEase(Ease.Linear));
        }


        private void Anim_ShowPanelsLinearFull()
        {
            // 1 frame = 32ms
            // Move panels 6 frames OutQuad
            // Fade Panels 6 frames OutQuad
            // Move Navigator 6 frames OutQuad

            // wait 2 frames
            // Scale SpinnyThing 4 frames OutQuad
            // Scale glass 4 frames OutQuad
            // Fade Glass 4 frames OutQuad
            // Move Gradient 4 frames OutQuad

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = true;
            radialCenterImage.gameObject.SetActive(false);
            radialCoverRing.SetActive(false);
            radialCoverBackground.SetActive(false);

            panelGroup.alpha = 0;
            panelGroupRect.eulerAngles = new Vector3(0, 0, 0);
            panelGroupRect.anchoredPosition = new Vector2(-250, 0);
            navigatorRect.anchoredPosition = new Vector2(1250, -400);

            spinnyThingRect.localScale = Vector3.one * 2;
            glassImage.rectTransform.localScale = Vector3.zero;
            glassImage.DOFade(0, 0);
            gradientRect.anchoredPosition = new Vector2(0, 400);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(1, frame * 3).SetEase(Ease.Linear));
            currentSequence.Join(panelGroupRect.DOAnchorPosX(0, frame * 6).SetEase(Ease.OutQuad));
            currentSequence.Join(navigatorRect.DOAnchorPosX(270, frame * 6).SetEase(Ease.OutQuad));

            currentSequence.Insert(frame * 2, spinnyThingRect.DOScale(1, frame * 4).SetEase(Ease.OutQuad));
            currentSequence.Insert(frame * 2, glassImage.rectTransform.DOScale(1, frame * 4).SetEase(Ease.OutQuad));
            currentSequence.Insert(frame * 2, glassImage.DOFade(1, frame * 4).SetEase(Ease.OutQuad));
            currentSequence.Insert(frame * 2, gradientRect.DOAnchorPosY(652.5f, frame * 4));
        }

        public void Anim_HidePanelsLinearFull()
        {
            // 1 frame = 32ms
            // Move panels 4 frames InQuad
            // Scale SpinnyThing 4 frames InQuad
            // Scale Glass 4 frames InQuad
            // Fade Glass 4 frames InQuad
            // Move Gradient 4 frames InQuad

            // wait 2 frames
            // Fade panels out 2 frames Linear
            // Move Navigator 6 frames 
            // Fade Navigator 6 frames Linear

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = true;
            radialCenterImage.gameObject.SetActive(false);
            radialCoverRing.SetActive(false);
            radialCoverBackground.SetActive(false);

            panelGroupRect.anchoredPosition = new Vector2(0, 0);
            panelGroupRect.eulerAngles = new Vector3(0, 0, 0);
            glassImage.rectTransform.localScale = Vector3.one;
            glassImage.DOFade(1, 0);
            gradientRect.anchoredPosition = new Vector2(0, 652.5f);

            panelGroup.alpha = 1;
            navigatorRect.anchoredPosition = new Vector2(270, -400);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroupRect.DOAnchorPosX(-250, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(spinnyThingRect.DOScale(2, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(glassImage.rectTransform.DOScale(0.5f, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(glassImage.DOFade(0, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(gradientRect.DOAnchorPosY(400, frame * 4).SetEase(Ease.InQuad));

            currentSequence.Insert(frame * 2, panelGroup.DOFade(0, frame * 2).SetEase(Ease.Linear));
            currentSequence.Insert(frame * 2, navigatorRect.DOAnchorPosX(1250, frame * 6).SetEase(Ease.InQuad));
        }


        private void Anim_ShowPanelsRadial()
        {
            // Scale Glass 8 frames OutQuad
            // Fade Glass 8 frames OutQuad
            // Fade radial center 4 frames InQuad

            // wait 4 frames
            // Spin panels 6 frames OutQuad
            // Fade panels 6 frames OutQuad

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = false;
            radialCenterImage.gameObject.SetActive(true);
            radialCoverRing.SetActive(true);
            radialCoverBackground.SetActive(true);

            panelGroupRect.anchoredPosition = new Vector2(0, 0);
            panelGroupRect.eulerAngles = new Vector3(0, 0, 120);
            panelGroup.alpha = 0;
            glassImage.rectTransform.localScale = Vector3.zero;
            glassImage.DOFade(0, 0);
            radialCenterImage.DOFade(0, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(glassImage.rectTransform.DOScale(1, frame * 8).SetEase(Ease.OutQuad));
            currentSequence.Join(glassImage.DOFade(1, frame * 8).SetEase(Ease.OutQuad));
            currentSequence.Join(radialCenterImage.DOFade(1, frame * 4).SetEase(Ease.InQuad));

            currentSequence.Insert(frame * 4,
                panelGroupRect.DORotate(new Vector3(0, 0, 0), frame * 6).SetEase(Ease.OutQuad));
            currentSequence.Insert(frame * 4, panelGroup.DOFade(1, frame * 6).SetEase(Ease.OutQuad));
        }

        private void Anim_HidePanelsRadial()
        {
            // Spin panels 3 frames Linear
            // Fade panels 3 frames OutQuad
            // Scale glass 6 frames Linear
            // Fade glass 6 frames Linear

            // wait 6 frames
            // Scale preview 4 frames InBounce

            const float frame = 0.032f;
            currentSequence.Kill(true);

            panelMask.enabled = false;
            radialCenterImage.gameObject.SetActive(true);
            radialCoverRing.SetActive(true);
            radialCoverBackground.SetActive(true);

            panelGroupRect.eulerAngles = new Vector3(0, 0, 0);
            panelGroup.alpha = 1;
            glassImage.rectTransform.localScale = Vector3.one;
            glassImage.DOFade(1, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(0, frame * 3).SetEase(Ease.OutQuad));
            currentSequence.Join(glassImage.rectTransform.DOScale(0, frame * 6).SetEase(Ease.Linear));
            currentSequence.Join(glassImage.DOFade(0, frame * 6).SetEase(Ease.Linear));
        }


        public void SetPrimaryPanel([NotNull] UIListItem item)
        {
            bool dynamic = item.itemType is UIListItem.ItemTypes.SubMenu &&
                           item.subtitleType is UIListItem.SubtitleTypes.Dynamic;

            primaryPanel.Title = item.title;
            primaryPanel.Subtitle = dynamic ? GetSelectedString(item) : item.subtitle;
            primaryPanel.SetRadialPanelColor(item);
        }

        public void GetPanels([NotNull] UIScreen screen, int currentIndex = 0)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, screen.listItems.Count);
            
            if (screen.listItems.Count == 0) return;

            if (screen.screenType is UIScreen.UIScreenType.Radial) getRadial();
            else getLinear();
            return;

            void getLinear()
            {
                linearPanelGroup.SetActive(true);
                radialPanelGroup.SetActive(false);

                SetPrimaryPanel(screen.listItems[currentIndex]);
                primaryPanel.SetType(screen.screenType);

                LinearCenterIndex = LinearHalfCount;

                for (int i = 0; i < linearPanels.Count; i++)
                {
                    OptionPanelLinear panel = linearPanels[i];
                    int itemIndex = currentIndex - LinearCenterIndex + i;

                    if (itemIndex >= screen.listItems.Count || itemIndex < 0)
                    {
                        panel.gameObject.SetActive(false);
                        continue;
                    }

                    UIListItem item = screen.listItems[itemIndex];
                    SetPanelLinear(screen, item, panel);

                    Vector2 position = GetLinearPosition(i);
                    float scale = GetLinearScale(i);

                    panel.rect.anchoredPosition = position;
                    panel.rect.localScale = Vector3.one * scale;
                    panel.gameObject.SetActive(true);
                }
            }

            void getRadial()
            {
                linearPanelGroup.SetActive(false);
                radialPanelGroup.SetActive(true);

                SetPrimaryPanel(screen.listItems[currentIndex]);
                primaryPanel.SetType(screen.screenType);

                RadialCenterIndex = RadialHalfCount;

                for (int i = 0; i < radialPanels.Count; i++)
                {
                    OptionPanelRadial panel = radialPanels[i];
                    int itemIndex = currentIndex - RadialCenterIndex + i;

                    if (itemIndex >= screen.listItems.Count || itemIndex < 0)
                    {
                        panel.gameObject.SetActive(false);
                        continue;
                    }

                    UIListItem item = screen.listItems[itemIndex];
                    SetPanelRadial(item, panel);

                    Vector3 angle = GetRadialAngle(i);
                    
                    panel.rect.eulerAngles = angle;
                    panel.gameObject.SetActive(true);
                }
            }
        }

        private static string GetSelectedString([NotNull] UIListItem item)
        {
            int settingsIndex = SettingsManager.Instance.PlayerSettings.GetParameter(item.settingsBinding);

            if (settingsIndex == -1)
            {
                Debug.LogWarning($"Setting \"{item.settingsBinding}\" was not found!");
                return "???";
            }

            if (item.nextScreen == null)
            {
                Debug.LogWarning($"NextScreen of [{item.title}] has not been set!");
                return "???";
            }

            if (item.nextScreen.listItems.Count == 0)
            {
                Debug.LogWarning($"NextScreen of [{item.title}] has no List Items!");
                return "???";
            }

            UIListItem selectedItem = item.nextScreen.listItems.FirstOrDefault(x => x.settingsValue == settingsIndex);

            if (selectedItem != null) return selectedItem.title;
            
            Debug.LogWarning($"No item with matching index [{settingsIndex}] was found!");
            return "???";
        }
        
        private static Vector2 GetLinearPosition(int index)
        {
            float[] posX = { 120,   55,   20,   20,   20,   55,  120 };
            float[] posY = { 350,  250,  150,    0, -150, -250, -350 };
            return new Vector2(posX[index], posY[index]);
        }

        private static float GetLinearScale(int index)
        {
            float[] scales = {0.85f, 0.85f, 1, 1, 1, 0.85f, 0.85f };
            return scales[index];
        }

        private static Vector3 GetRadialAngle(int index)
        {
            // This is a little jank but... it works.
            float[] angles =
            {
                -99, -99, -99, -99, -99, -99, -99, -99, -81, -63, -45, -27, 0, 27, 45, 63, 81, 99, 117, 135, 153, 171,
                189, 207, 225,
            };
            return new Vector3(0, 0, angles[index]);
        }

        private static void SetPanelLinear([NotNull] UIScreen screen, [NotNull] UIListItem item,
            [NotNull] OptionPanelLinear panel)
        {
            bool dynamic = item.itemType is UIListItem.ItemTypes.SubMenu &&
                           item.subtitleType is UIListItem.SubtitleTypes.Dynamic;

            panel.Title = item.title;
            panel.Subtitle = dynamic ? GetSelectedString(item) : item.subtitle;
            panel.SetType(screen.screenType);
        }

        private static void SetPanelRadial([NotNull] UIListItem item, [NotNull] OptionPanelRadial panel)
        {
            panel.Title = item.title;
            panel.SetRadialPanelColor(item);
        }
    }
}
