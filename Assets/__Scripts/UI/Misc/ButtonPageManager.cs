using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{   
    public class ButtonPageManager : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> buttonGroups;
        private const float Scale = 1.5f;
        private const float TweenDuration = 0.15f;
        private const Ease TweenEase = Ease.InOutQuad;

        public void SetActiveButtons(int index)
        {
            for (int i = 0; i < buttonGroups.Count; i++)
                buttonGroups[i].gameObject.SetActive(i == index);
        }
        public async void SwitchButtons(int index)
        {
            if (index < 0 || index >= buttonGroups.Count)
                throw new ArgumentException($"Index out of range ({index}, {buttonGroups.Count})");

            foreach (RectTransform group in buttonGroups)
            {
                group.localScale = Vector3.one;
                group.DOScale(Scale, TweenDuration).SetEase(TweenEase);
            }

            await Awaitable.WaitForSecondsAsync(TweenDuration);

            SetActiveButtons(index);
            
            buttonGroups[index].localScale = Vector3.one * Scale;
            buttonGroups[index].DOScale(1, TweenDuration).SetEase(TweenEase);
        }
    }
}