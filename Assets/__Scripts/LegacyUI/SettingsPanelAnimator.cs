using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{
    public class SettingsPanelAnimator : MonoBehaviour
    {
        [SerializeField] private SettingsPanel selectedPanel;
        [SerializeField] private List<SettingsPanel> panels;
        private int centerIndex = 0;
        private const float duration = 0.1f;
        private Ease ease = Ease.InOutCubic;
        public enum MoveDirection { Up = 1, Down = -1}
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                MovePanels(MoveDirection.Down);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                MovePanels(MoveDirection.Up);
            }
        }

        void Awake()
        {
            UpdatePanels(true);
        }

        public void MovePanels(MoveDirection direction)
        {
            centerIndex += (int) direction;
            UpdatePanels();
        }

        void UpdatePanels(bool snap = false)
        {
            // Set SelectedPanel's text here

            for (int i = 0; i < panels.Count; i++)
            {
                if (snap)
                {                    
                    panels[i].rect.anchoredPosition = GetPosition(i - centerIndex);
                    panels[i].rect.localScale = Vector3.one * GetScale(i - centerIndex);
                }
                else
                {
                    panels[i].rect.DOAnchorPos(GetPosition(i - centerIndex), duration).SetEase(ease);
                    panels[i].rect.DOScale(GetScale(i - centerIndex), duration).SetEase(ease);
                }
            }
        }

        Vector2 GetPosition(int index)
        {
            float[] yList = {-350, -250, -150, 0, 150, 250, 350};
            int clampedIndex = Mathf.Clamp(index + 3, 0, 6);

            float x = -570 + 45 * Mathf.Abs(index);
            float y = yList[clampedIndex];

            return new(x, y);
        }

        float GetScale(int index)
        {
            return Mathf.Clamp01(1.2f - Mathf.Abs(index) * 0.2f);
        }
    }

}