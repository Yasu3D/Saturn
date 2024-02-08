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

        public void OnConfirm()
        {

        }

        public void OnBack()
        {
            screenStack.Pop();
            indexStack.Pop();

            if (screenStack.Count == 0)
            {
                SceneSwitcher.Instance.LoadScene("_SongSelect");
            }
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

            if (Input.GetKeyDown(KeyCode.UpArrow)) panelAnimator.Anim_ShowPanels();
            if (Input.GetKeyDown(KeyCode.DownArrow)) panelAnimator.Anim_HidePanels();
        }
    }

    [Serializable]
    public class UIListItem
    {
        public string Title;
        public string Subtitle;
    }
}
