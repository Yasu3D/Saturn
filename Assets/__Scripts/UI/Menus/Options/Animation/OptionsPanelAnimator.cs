using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using SaturnGame.Settings;
using static UnityEngine.Rendering.DebugUI;
using UnityEditorInternal.VersionControl;

namespace SaturnGame.UI
{
    public class OptionsPanelAnimator : MonoBehaviour
    {
        [SerializeField] private UIPanelObjectPool panelPool;
        [SerializeField] private RectTransform activePanels;

        [SerializeField] private OptionPanel primaryPanel;

        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private RectTransform panelGroupRect;
        [SerializeField] private RectTransform gradientRect;
        [SerializeField] private RectTransform spinnyThingRect;
        [SerializeField] private RectTransform headerRect;
        [SerializeField] private RectTransform navigatorRect;
        [SerializeField] private RectMask2D panelMask;
        [SerializeField] private Image glassImage;
        [SerializeField] private Image radialCenterImage;
        [SerializeField] private GameObject radialCoverRing;
        [SerializeField] private GameObject radialCoverBackground;

        private Sequence currentSequence;

        private float[] positionsY = { 0, -150, -250, -350 };
        private float[] positionsX = { 20, 20, 55, 120 };
        private float[] scales = { 1, 1, 0.85f, 0.85f };

        private float[] angles = { -99, -81, -63, -45, -27, 0, 27, 45, 63, 81, 99, 117, 135, 153, 171, 189, 207, 225 };
        private Vector2 centerPoint = new(0, 0);

        public void Anim_ShiftPanels(int selectedIndex, UIScreen screen)
        {
            const float duration = 0.05f;
            Ease ease = Ease.Linear;

            for (int i = 0; i < panelPool.ActiveObjects.Count; i++)
            {
                int distance = i - selectedIndex;
                int sign = (int)Mathf.Sign(distance);
                var panel = panelPool.ActiveObjects[i];

                if (screen.ScreenType is not UIScreen.UIScreenType.Radial)
                {
                    int index = Mathf.Clamp(Mathf.Abs(distance), 0, 3);
                    Vector2 position = new(positionsX[index], sign * positionsY[index]);
                    float scale = scales[index];

                    panel.rect.DOAnchorPos(position, duration).SetEase(ease);
                    panel.rect.DOScale(scale, duration).SetEase(ease);
                }

                if (screen.ScreenType is UIScreen.UIScreenType.Radial)
                {
                    panel.rect.anchoredPosition = centerPoint;
                    panel.rect.localScale = Vector3.one;

                    int index = Mathf.Clamp(distance + 5, 0, 17);
                    float angle = angles[index];

                    panel.rect.DORotate(new Vector3(0, 0, angle), duration, RotateMode.Fast).SetEase(ease);
                }
            }
        }

        public void Anim_ShowPanels(UIScreen previous, UIScreen next)
        {
            bool prevLinear = previous.ScreenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;
            bool nextLinear = next.ScreenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;

            if (!nextLinear)
            {
                Debug.Log("Show Radial");
                Anim_ShowPanelsRadial();
                return;
            }

            if (prevLinear)
            {
                Debug.Log("Show Partial");
                Anim_ShowPanelsLinearPartial();
            }
            else
            {
                Debug.Log("Show Full");
                Anim_ShowPanelsLinearFull();
            }

            // Linear -> Linear => Partial
            // Linear -> Radial => Radial
            // Radial -> Linear => Full
            // Radial -> Radial => Radial
        }

        public void Anim_HidePanels(UIScreen previous, UIScreen next)
        {
            bool prevLinear = previous.ScreenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;
            bool nextLinear = next.ScreenType is UIScreen.UIScreenType.LinearSimple or UIScreen.UIScreenType.LinearDetailed;

            if (!prevLinear)
            {
                Debug.Log("Hide Radial");
                Anim_HidePanelsRadial();
                return;
            }
            
            if (nextLinear)
            {
                Debug.Log("Hide Partial");
                Anim_HidePanelsLinearPartial();
            }
            else
            {
                Debug.Log("Hide Full");
                Anim_HidePanelsLinearFull();
            }

            // Linear -> Linear => Partial
            // Linear -> Radial => Full
            // Radial -> Linear => Radial
            // Radial -> Radial => Radial
        }


        public void Anim_ShowPanelsLinearPartial()
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
            panelGroupRect.anchoredPosition = new(-250, 0);
            panelGroupRect.eulerAngles = new(0, 0, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(1, frame * 6).SetEase(Ease.OutQuad));
            currentSequence.Join(panelGroupRect.DOAnchorPosX(0, frame * 6).SetEase(Ease.OutQuad));
        }

        public void Anim_HidePanelsLinearPartial()
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

            panelGroupRect.anchoredPosition = new(0, 0);
            panelGroupRect.eulerAngles = new(0, 0, 0);
            panelGroup.alpha = 1;

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroupRect.DOAnchorPosX(-250, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Insert(frame * 2, panelGroup.DOFade(0, frame * 2).SetEase(Ease.Linear));
        }


        public void Anim_ShowPanelsLinearFull()
        {
            // 1 frame = 32ms
            // Move panels 6 frames OutQuad
            // Fade Panels 6 frames OutQuad
            // Move Navigator 6 frames OutQuad

            // wait 2 frames
            // Scale Spinnything 4 frames OutQuad
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
            panelGroupRect.eulerAngles = new(0, 0, 0);
            panelGroupRect.anchoredPosition = new(-250, 0);
            navigatorRect.anchoredPosition = new(1250, -400);

            spinnyThingRect.localScale = Vector3.one * 2;
            glassImage.rectTransform.localScale = Vector3.zero;
            glassImage.DOFade(0, 0);
            gradientRect.anchoredPosition = new(0, 400);

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

            panelGroupRect.anchoredPosition = new(0, 0);
            panelGroupRect.eulerAngles = new(0, 0, 0);
            glassImage.rectTransform.localScale = Vector3.one;
            glassImage.DOFade(1, 0);
            gradientRect.anchoredPosition = new(0, 652.5f);

            panelGroup.alpha = 1;
            navigatorRect.anchoredPosition = new(270, -400);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroupRect.DOAnchorPosX(-250, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(spinnyThingRect.DOScale(2, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(glassImage.rectTransform.DOScale(0.5f, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(glassImage.DOFade(0, frame * 4).SetEase(Ease.InQuad));
            currentSequence.Join(gradientRect.DOAnchorPosY(400, frame * 4).SetEase(Ease.InQuad));

            currentSequence.Insert(frame * 2, panelGroup.DOFade(0, frame * 2).SetEase(Ease.Linear));
            currentSequence.Insert(frame * 2, navigatorRect.DOAnchorPosX(1250, frame * 6).SetEase(Ease.InQuad));
        }


        public void Anim_ShowPanelsRadial()
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

            panelGroupRect.anchoredPosition = new(0, 0);
            panelGroupRect.eulerAngles = new(0, 0, 270);
            panelGroup.alpha = 0;
            glassImage.rectTransform.localScale = Vector3.zero;
            glassImage.DOFade(0, 0);
            radialCenterImage.DOFade(0, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(glassImage.rectTransform.DOScale(1, frame * 8).SetEase(Ease.OutQuad));
            currentSequence.Join(glassImage.DOFade(1, frame * 8).SetEase(Ease.OutQuad));
            currentSequence.Join(radialCenterImage.DOFade(1, frame * 4).SetEase(Ease.InQuad));

            currentSequence.Insert(frame * 4, panelGroupRect.DORotate(new(0, 0, 0), frame * 6).SetEase(Ease.OutQuad));
            currentSequence.Insert(frame * 4, panelGroup.DOFade(1, frame * 6).SetEase(Ease.OutQuad));

        }

        public void Anim_HidePanelsRadial()
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

            panelGroupRect.eulerAngles = new(0, 0, 0);
            panelGroup.alpha = 1;
            glassImage.rectTransform.localScale = Vector3.one;
            glassImage.DOFade(1, 0);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(0, frame * 3).SetEase(Ease.OutQuad));
            currentSequence.Join(glassImage.rectTransform.DOScale(0, frame * 6).SetEase(Ease.Linear));
            currentSequence.Join(glassImage.DOFade(0, frame * 6).SetEase(Ease.Linear));
        }


        public void SetSelectedPanel(UIListItem item)
        {
            primaryPanel.Title = item.Title;

            if (item.SubtitleType is UIListItem.SubtitleTypes.Text)
                primaryPanel.Subtitle = item.Subtitle;

            // This is really really hacky and bad. So.. WIP I guess?
            if (item.ItemType is UIListItem.ItemTypes.SubMenu && item.SubtitleType is UIListItem.SubtitleTypes.Binding && item.Binding != "")
            {
                int settingsValue = SettingsManager.Instance.PlayerSettings.GetParameter(item.Binding);
                string subtitle = item.NextScreen.ListItems[settingsValue].Title;
                primaryPanel.Subtitle = subtitle;
            }

            primaryPanel.SetRadialPanelColor(item);
        }

        public void GetPanels(UIScreen screen, int selectedIndex = 0)
        {
            int activeCount = panelPool.ActiveObjects.Count;
            int newCount = screen.ListItems.Count;
            int difference = activeCount - newCount;

            switch (difference)
            {
                case > 0:
                    for (int i = 0; i < difference; i++)
                        panelPool.ReleaseObject(panelPool.ActiveObjects[0]);
                    break;

                case < 0:
                    for (int i = 0; i < -difference; i++)
                    {
                        var panel = panelPool.GetObject();
                        panel.rect.SetParent(activePanels);
                        panel.gameObject.SetActive(true);
                    }
                    break;

                default:
                    break;
            }

            for (int i = 0; i < panelPool.ActiveObjects.Count; i++)
            {
                var panel = panelPool.ActiveObjects[i];
                var listItem = screen.ListItems[i];


                panel.Title = listItem.Title;

                if (listItem.SubtitleType is UIListItem.SubtitleTypes.Text)
                    panel.Subtitle = listItem.Subtitle;

                // This is really really hacky and bad. So.. WIP I guess?
                if (listItem.ItemType is UIListItem.ItemTypes.SubMenu && listItem.SubtitleType is UIListItem.SubtitleTypes.Binding && listItem.Binding != "")
                {
                    int settingsValue = SettingsManager.Instance.PlayerSettings.GetParameter(listItem.Binding);
                    string subtitle = listItem.NextScreen.ListItems[settingsValue].Title;
                    panel.Subtitle = subtitle;
                }

                panel.SetType(screen.ScreenType);
                panel.SetRadialPanelColor(listItem);

                int distance = i - selectedIndex;
                int sign = (int) Mathf.Sign(distance);

                if (screen.ScreenType is UIScreen.UIScreenType.Radial)
                {
                    panel.rect.anchoredPosition = centerPoint;
                    panel.rect.localScale = Vector3.one;

                    int index = Mathf.Clamp(distance + 5, 0, 17);
                    float angle = angles[index];

                    panel.rect.eulerAngles = new Vector3(0, 0, angle);
                }

                if (screen.ScreenType is not UIScreen.UIScreenType.Radial)
                {
                    int index = Mathf.Clamp(Mathf.Abs(distance), 0, 3);
                    Vector2 position = new(positionsX[index], sign * positionsY[index]);
                    float scale = scales[index];

                    panel.rect.eulerAngles = Vector3.zero;
                    panel.rect.anchoredPosition = position;
                    panel.rect.localScale = Vector3.one * scale;
                }
            }

            primaryPanel.SetType(screen.ScreenType);
        }
    }
}
