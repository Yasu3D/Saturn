using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.UI
{
    public class OptionsLogic : MonoBehaviour
    {
        private enum MenuState
        {
            Idle,
            MenuSwitch
        }

        [SerializeField] private OptionsPanelAnimator panelAnimator;
        [SerializeField] private UIScreen startScreen;
        private UIAudioController UIAudio => UIAudioController.Instance;
        private Stack<UIScreen> screenStack = new();
        private UIScreen currentScreen => screenStack.Peek();
        private Stack<int> indexStack = new(new List<int>{0});
        private int currentIndex
        {
            get => indexStack.Peek();
            set
            {
                indexStack.Pop();
                indexStack.Push(value);
            }
        }
        private MenuState state = MenuState.Idle;

        void Awake()
        {
            screenStack.Push(startScreen);
            panelAnimator.GetPanels(startScreen);
            panelAnimator.SetSelectedPanel(startScreen.ListItems[currentIndex]);
        }

        public async void OnConfirm()
        {
            if (state is MenuState.MenuSwitch) return;

            var prevScreen = currentScreen;
            var nextScreen = currentScreen.ListItems[currentIndex].NextScreen;
            if (nextScreen == null) return;

            UIAudio.PlaySound(UIAudioController.UISound.Confirm);

            screenStack.Push(nextScreen);
            indexStack.Push(0);

            panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
            state = MenuState.MenuSwitch;

            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(currentScreen);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[0]);
            panelAnimator.Anim_ShowPanels(prevScreen, nextScreen);
            state = MenuState.Idle;
        }

        public async void OnBack()
        {
            if (state is MenuState.MenuSwitch) return;

            UIAudio.PlaySound(UIAudioController.UISound.Back);

            if (screenStack.Count <= 1 || indexStack.Count <= 1)
            {
                panelAnimator.Anim_HidePanelsLinearFull();
                SceneSwitcher.Instance.LoadScene("_SongSelect");
                return;
            }

            // might be a little confusing but does the trick
            var prevScreen = currentScreen;
            screenStack.Pop();
            indexStack.Pop();
            var nextScreen = currentScreen;

            panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
            state = MenuState.MenuSwitch;

            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(currentScreen, currentIndex);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
            panelAnimator.Anim_ShowPanels(prevScreen, nextScreen);
            state = MenuState.Idle;
        }

        public void OnNavigateLeft()
        {
            if (state is MenuState.MenuSwitch) return;
            if (screenStack.Count == 0 || indexStack.Count == 0) return;

            int newIndex = Mathf.Max(currentIndex - 1, 0);
            if (currentIndex != newIndex)
            {
                UIAudio.PlaySound(UIAudioController.UISound.Navigate);
                currentIndex = newIndex;
            }

            panelAnimator.Anim_ShiftPanels(currentIndex, currentScreen);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
        }
        
        public void OnNavigateRight()
        {
            if (state is MenuState.MenuSwitch) return;
            if (screenStack.Count == 0 || indexStack.Count == 0) return;

            int newIndex = Mathf.Min(currentIndex + 1, currentScreen.ListItems.Count - 1);
            if (currentIndex != newIndex)
            {
                UIAudio.PlaySound(UIAudioController.UISound.Navigate);
                currentIndex = newIndex;
            }

            panelAnimator.Anim_ShiftPanels(currentIndex, currentScreen);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
        }

        public void OnDefault() {}

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
            if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
            if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
            if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
        }
    }
}
