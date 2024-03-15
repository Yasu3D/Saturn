using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SaturnGame.UI
{   
    public class ButtonPageManager : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> buttonGroups;
        private const float scale = 1.5f;
        private const float tweenDuration = 0.15f;
        private readonly Ease tweenEase = Ease.InOutQuad;

        public void SetActiveButtons(int index)
        {
            for (int i = 0; i < buttonGroups.Count; i++)
            {
                buttonGroups[i].gameObject.SetActive(i == index);
            }
        }
        public async void SwitchButtons(int index)
        {
            if (index < 0 || index >= buttonGroups.Count)
                throw new ArgumentException($"Index out of range ({index}, {buttonGroups.Count})");

            for (int i = 0; i < buttonGroups.Count; i++)
            {
                buttonGroups[i].localScale = Vector3.one;
                buttonGroups[i].DOScale(scale, tweenDuration).SetEase(tweenEase);
            }

            await Awaitable.WaitForSecondsAsync(tweenDuration);

            SetActiveButtons(index);
            
            buttonGroups[index].localScale = Vector3.one * scale;
            buttonGroups[index].DOScale(1, tweenDuration).SetEase(tweenEase);
        }
    }
}