using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

        private Sequence currentSequence;

        private float[] positionsY = { 0, -150, -250, -350 };
        private float[] positionsX = { 20, 20, 55, 120 };
        private float[] scales = { 1, 1, 0.85f, 0.85f };

        private float[] angles = { -99, -81, -63, -45, -27, 0, 27, 45, 63, 81, 99, 117, 135, 153, 171, 189, 207, 225 };
        private Vector2 centerPoint = new(0, -12);

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

        public void Anim_ShowPanels(UIScreen newScreen)
        {
            switch (newScreen.ScreenType)
            {
                case UIScreen.UIScreenType.LinearSimple:
                case UIScreen.UIScreenType.LinearDetailed:
                    Anim_ShowPanelsLinear();
                    break;

                case UIScreen.UIScreenType.Radial:
                    Anim_ShowPanelsRadial();
                    break;
            }
        }

        public void Anim_HidePanels(UIScreen lastScreen)
        {
            switch (lastScreen.ScreenType)
            {
                case UIScreen.UIScreenType.LinearSimple:
                case UIScreen.UIScreenType.LinearDetailed:
                    Anim_HidePanelsLinear();
                    break;

                case UIScreen.UIScreenType.Radial:
                    Anim_HidePanelsRadial();
                    break;
            }
        }

        public void Anim_ShowPanelsLinear()
        {
            const float duration = 0.2f;
            Ease ease = Ease.InQuad;

            currentSequence.Kill(true);

            panelGroup.alpha = 0;
            panelGroupRect.anchoredPosition = new(-250, 12.5f);
            gradientRect.anchoredPosition = new(0, 400);
            headerRect.anchoredPosition = new(0, 250);
            spinnyThingRect.localScale = Vector3.one * 2;
            navigatorRect.anchoredPosition = new(1250, -400);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(1, duration).SetEase(ease));
            currentSequence.Join(panelGroupRect.DOAnchorPosX(0, duration).SetEase(ease));
            currentSequence.Join(gradientRect.DOAnchorPosY(665, duration).SetEase(ease));
            currentSequence.Join(headerRect.DOAnchorPosX(-420, duration).SetEase(ease));
            currentSequence.Join(spinnyThingRect.DOScale(1, duration).SetEase(ease));
            currentSequence.Join(navigatorRect.DOAnchorPosX(270, duration).SetEase(ease));
        }

        public void Anim_ShowPanelsRadial()
        {
            const float duration = 0.2f;
            Ease ease = Ease.InQuad;

            currentSequence.Kill(true);

            panelGroup.alpha = 0;
            panelGroupRect.anchoredPosition = new(0, 12.5f);
            panelGroupRect.localScale = Vector3.zero;

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(1, duration).SetEase(ease));
            currentSequence.Join(panelGroupRect.DOScale(1, duration).SetEase(ease));
        }

        public void Anim_HidePanelsLinear()
        {
            const float duration = 0.2f;
            Ease ease = Ease.InQuad;

            currentSequence.Kill(true);

            panelGroup.alpha = 1;
            panelGroupRect.anchoredPosition = new(0, 12.5f);
            gradientRect.anchoredPosition = new(0, 665);
            headerRect.anchoredPosition = new(-420, 250);
            spinnyThingRect.localScale = Vector3.one;
            navigatorRect.anchoredPosition = new(270, -400);

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(0, duration).SetEase(ease));
            currentSequence.Join(panelGroupRect.DOAnchorPosX(-250, duration).SetEase(ease));
            currentSequence.Join(gradientRect.DOAnchorPosY(400, duration).SetEase(ease));
            currentSequence.Join(headerRect.DOAnchorPosX(0, duration).SetEase(ease));
            currentSequence.Join(spinnyThingRect.DOScale(2, duration).SetEase(ease));
            currentSequence.Join(navigatorRect.DOAnchorPosX(1250, duration).SetEase(ease));
        }

        public void Anim_HidePanelsRadial()
        {
            const float duration = 0.2f;
            Ease ease = Ease.InQuad;

            currentSequence.Kill(true);

            panelGroup.alpha = 1;
            panelGroupRect.anchoredPosition = new(0, 12.5f);
            panelGroupRect.localScale = Vector3.one;

            currentSequence = DOTween.Sequence();
            currentSequence.Join(panelGroup.DOFade(0, duration).SetEase(ease));
            currentSequence.Join(panelGroupRect.DOScale(0, duration).SetEase(ease));
        }

        public void SetSelectedPanel(UIListItem item)
        {
            primaryPanel.Title = item.Title;
            primaryPanel.Subtitle = item.Subtitle;
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
                panel.Subtitle = listItem.Subtitle;
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
