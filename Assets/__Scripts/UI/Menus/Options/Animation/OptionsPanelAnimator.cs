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

        [SerializeField] private OptionPanel selectedPanel;

        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private RectTransform panelGroupRect;
        [SerializeField] private RectTransform gradientRect;
        [SerializeField] private RectTransform navigatorRect;
        [SerializeField] private RectTransform spinnyThingRect;
        [SerializeField] private RectTransform glassRect;
        [SerializeField] private RectTransform headerRect;

        private float[] positionsY = {0, -150, -250, -350};
        private float[] positionsX = {-520, -520, -485, -420};
        private float[] scales = {1, 1, 0.85f, 0.85f};

        public void Anim_ShiftPanels(int selectedIndex)
        {
            const float duration = 0.05f;
            Ease ease = Ease.Linear;

            for (int i = 0; i < panelPool.ActiveObjects.Count; i++)
            {
                int distance = i - selectedIndex;
                int sign = (int) Mathf.Sign(distance);
                int index = Mathf.Clamp(Mathf.Abs(distance), 0, 3);

                Vector2 position = new(positionsX[index], sign * positionsY[index]);
                float scale = scales[index];
                var panel = panelPool.ActiveObjects[i];

                panel.rect.DOAnchorPos(position, duration).SetEase(ease);
                panel.rect.DOScale(scale, duration).SetEase(ease);
            }
        }

        public void Anim_ShowPanels()
        {
            const float duration = 0.075f;
            Ease ease = Ease.InQuad;

            panelGroup.DOFade(1, duration).SetEase(ease);
            panelGroupRect.DOAnchorPosX(0, duration).SetEase(ease);
            //gradientRect.DOAnchorPosY(665, duration).SetEase(ease);
        }

        public void Anim_HidePanels()
        {
            const float duration = 0.075f;
            Ease ease = Ease.InQuad;

            panelGroup.DOFade(0, duration).SetEase(ease);
            panelGroupRect.DOAnchorPosX(-250, duration).SetEase(ease);
            //gradientRect.DOAnchorPosY(400, duration).SetEase(ease);
        }

        public void Anim_HideAll()
        {
            const float duration = 0.075f;
            Ease ease = Ease.InQuad;

            panelGroup.DOFade(0, duration).SetEase(ease);
            panelGroupRect.DOAnchorPosX(-250, duration).SetEase(ease);
            gradientRect.DOAnchorPosY(400, duration).SetEase(ease);
            navigatorRect.DOAnchorPosX(1250, duration).SetEase(ease);
            headerRect.DOAnchorPosX(0, duration).SetEase(ease);
            glassRect.DOScale(0, duration).SetEase(ease);
            spinnyThingRect.DOScale(2, duration).SetEase(ease);
        }

        public void SetSelectedPanel(UIListItem item)
        {
            selectedPanel.Title = item.Title;
            selectedPanel.Subtitle = item.Subtitle;
        }

        public void GetPanels(List<UIListItem> items, int selectedIndex = 0)
        {
            int activeCount = panelPool.ActiveObjects.Count;
            int newCount = items.Count;
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

            Debug.Log($"{difference}  {panelPool.ActiveObjects.Count}  {items.Count}");

            for (int i = 0; i < panelPool.ActiveObjects.Count; i++)
            {
                var panel = panelPool.ActiveObjects[i];

                int distance = i - selectedIndex;
                int sign = (int) Mathf.Sign(distance);
                int index = Mathf.Clamp(Mathf.Abs(distance), 0, 3);

                Vector2 position = new(positionsX[index], sign * positionsY[index]);
                float scale = scales[index];
                panel.rect.anchoredPosition = position;
                panel.rect.localScale = Vector3.one * scale;

                panel.Title = items[i].Title;
                panel.Subtitle = items[i].Subtitle;
            }
        }
    }
}
