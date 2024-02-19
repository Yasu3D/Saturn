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
        private static UIAudioController UIAudio => UIAudioController.Instance;
        private readonly Stack<UIScreen> screenStack = new();
        private UIScreen CurrentScreen => screenStack.Peek();
        private readonly Stack<int> indexStack = new(new List<int>{0});
        private int CurrentIndex
        {
            get => indexStack.Peek();
            set
            {
                indexStack.Pop();
                indexStack.Push(value);
            }
        }
        private MenuState state = MenuState.Idle;

        private void Awake()
        {
            screenStack.Push(startScreen);
            panelAnimator.GetPanels(startScreen);
            panelAnimator.SetPrimaryPanel(startScreen.listItems[CurrentIndex]);
        }

        public async void OnConfirm()
        {
            if (state is MenuState.MenuSwitch) return;

            var prevScreen = CurrentScreen;
            var nextScreen = CurrentScreen.listItems[CurrentIndex].nextScreen;
            if (nextScreen == null) return;

            UIAudio.PlaySound(UIAudioController.UISound.Confirm);

            screenStack.Push(nextScreen);
            indexStack.Push(0);

            panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
            state = MenuState.MenuSwitch;

            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(CurrentScreen);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[0]);
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
            var prevScreen = CurrentScreen;
            screenStack.Pop();
            indexStack.Pop();
            var nextScreen = CurrentScreen;

            panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
            state = MenuState.MenuSwitch;

            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(CurrentScreen, CurrentIndex);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[CurrentIndex]);
            panelAnimator.Anim_ShowPanels(prevScreen, nextScreen);
            state = MenuState.Idle;
        }

        public void OnNavigateLeft()
        {
            if (state is MenuState.MenuSwitch) return;
            if (screenStack.Count == 0 || indexStack.Count == 0) return;

            int newIndex = Mathf.Max(CurrentIndex - 1, 0);
            if (CurrentIndex == newIndex) return;

            CurrentIndex = newIndex;
            UIAudio.PlaySound(UIAudioController.UISound.Navigate);
            panelAnimator.Anim_ShiftPanels(OptionsPanelAnimator.MoveDirection.Up, CurrentIndex, CurrentScreen);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[CurrentIndex]);
        }
        
        public void OnNavigateRight()
        {
            if (state is MenuState.MenuSwitch) return;
            if (screenStack.Count == 0 || indexStack.Count == 0) return;

            int newIndex = Mathf.Min(CurrentIndex + 1, CurrentScreen.listItems.Count - 1);
            if (CurrentIndex == newIndex) return;

            CurrentIndex = newIndex;
            UIAudio.PlaySound(UIAudioController.UISound.Navigate);
            panelAnimator.Anim_ShiftPanels(OptionsPanelAnimator.MoveDirection.Down, CurrentIndex, CurrentScreen);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[CurrentIndex]);
        }

        public void OnDefault() {}

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
            if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
            if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
            if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
        }
    }
}
