using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.UI
{
    public class OptionsLogic : MonoBehaviour
    {
        [SerializeField] private OptionsPanelAnimator panelAnimator;
        [SerializeField] private UIScreen startScreen;

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

        void Awake()
        {
            screenStack.Push(startScreen);
            panelAnimator.GetPanels(startScreen.ListItems);
            panelAnimator.SetSelectedPanel(startScreen.ListItems[currentIndex]);
        }

        public async void OnConfirm()
        {
            var nextScreen = currentScreen.ListItems[currentIndex].NextScreen;
            if (nextScreen == null) return;

            screenStack.Push(nextScreen);
            indexStack.Push(0);

            panelAnimator.Anim_HidePanels();
            await Awaitable.WaitForSecondsAsync(0.1f);
            panelAnimator.GetPanels(currentScreen.ListItems);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[0]);
            panelAnimator.Anim_ShowPanels();
        }

        public async void OnBack()
        {
            if (screenStack.Count <= 1 || indexStack.Count <= 1)
            {
                panelAnimator.Anim_HideAll();
                SceneSwitcher.Instance.LoadScene("_SongSelect");
                return;
            }

            screenStack.Pop();
            indexStack.Pop();

            panelAnimator.Anim_HidePanels();
            await Awaitable.WaitForSecondsAsync(0.1f);
            panelAnimator.GetPanels(currentScreen.ListItems, currentIndex);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
            panelAnimator.Anim_ShowPanels();
        }

        public void OnNavigateLeft()
        {
            if (screenStack.Count == 0 || indexStack.Count == 0) return;
            currentIndex = Mathf.Max(currentIndex - 1, 0);
            panelAnimator.Anim_ShiftPanels(currentIndex);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
        }
        
        public void OnNavigateRight()
        {
            if (screenStack.Count == 0 || indexStack.Count == 0) return;
            currentIndex = Mathf.Min(currentIndex + 1, currentScreen.ListItems.Count - 1);
            panelAnimator.Anim_ShiftPanels(currentIndex);
            panelAnimator.SetSelectedPanel(currentScreen.ListItems[currentIndex]);
        }

        public void OnDefault() {}

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
            if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
            if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
            if (Input.GetKeyDown(KeyCode.Escape)) OnBack();

            if (Input.GetKeyDown(KeyCode.X)) panelAnimator.Anim_HideAll();
        }
    }
}
