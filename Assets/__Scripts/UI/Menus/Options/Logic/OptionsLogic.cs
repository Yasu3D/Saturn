using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.UI
{
    public class OptionsLogic : MonoBehaviour
    {
        private enum MenuState
        {
            Idle,
            MenuSwitch,
        }

        [SerializeField] private OptionsPanelAnimator panelAnimator;
        [SerializeField] private UIScreen startScreen;
        [SerializeField] private GameObject revertButton;
        
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

        private async void OnConfirm()
        {
            if (state is MenuState.MenuSwitch) return;

            UIListItem selectedItem = CurrentScreen.listItems[CurrentIndex];
            UIScreen prevScreen = CurrentScreen;

            switch (selectedItem.itemType)
            {
                case UIListItem.ItemTypes.SubMenu:
                {
                    UIScreen nextScreen = selectedItem.nextScreen;
                    if (nextScreen == null || nextScreen.listItems.Count == 0) return;

                    UIAudio.PlaySound(UIAudioController.UISound.Confirm);

                    screenStack.Push(nextScreen);
                    indexStack.Push(0);

                    panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
                    state = MenuState.MenuSwitch;

                    // Jumps to Panel with selected option.
                    if (nextScreen.screenType is UIScreen.UIScreenType.Radial)
                    {
                        string parameter = nextScreen.listItems[0].settingsParameter;
                        int value = SettingsManager.Instance.PlayerSettings.GetParameter(parameter);

                        UIListItem item = nextScreen.listItems.FirstOrDefault(x => x.settingsValue == value);
                        if (item != null) CurrentIndex = nextScreen.listItems.IndexOf(item);
                    }
                    
                    break;
                }

                case UIListItem.ItemTypes.ValueSetter:
                {
                    if (selectedItem.settingsParameter == "") return;

                    UIAudio.PlaySound(UIAudioController.UISound.Confirm);
                    SettingsManager.Instance.PlayerSettings.SetParameter(selectedItem.settingsParameter,
                        selectedItem.settingsValue);

                    if (screenStack.Count <= 1 || indexStack.Count <= 1) return;

                    screenStack.Pop();
                    indexStack.Pop();

                    panelAnimator.Anim_HidePanels(prevScreen, CurrentScreen);
                    state = MenuState.MenuSwitch;
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            
            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(CurrentScreen, CurrentIndex);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[CurrentIndex]);
            panelAnimator.Anim_ShowPanels(prevScreen, CurrentScreen);
            ToggleRevertButton(CurrentScreen);
            state = MenuState.Idle;
        }

        private async void OnBack()
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
            UIScreen prevScreen = CurrentScreen;
            screenStack.Pop();
            indexStack.Pop();
            UIScreen nextScreen = CurrentScreen;

            panelAnimator.Anim_HidePanels(prevScreen, nextScreen);
            ToggleRevertButton(nextScreen);
            state = MenuState.MenuSwitch;

            await Awaitable.WaitForSecondsAsync(0.25f);
            panelAnimator.GetPanels(CurrentScreen, CurrentIndex);
            panelAnimator.SetPrimaryPanel(CurrentScreen.listItems[CurrentIndex]);
            panelAnimator.Anim_ShowPanels(prevScreen, nextScreen);
            state = MenuState.Idle;
        }

        private void OnNavigateLeft()
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

        private void OnNavigateRight()
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

        private async void OnRevert()
        {
            UIListItem selectedItem = CurrentScreen.listItems[CurrentIndex];
            if (selectedItem.settingsParameter == "") return;

            int defaultIndex = CurrentScreen.defaultItemIndex;
            
            bool outOfBounds = defaultIndex >= CurrentScreen.listItems.Count || defaultIndex < 0;
            bool sameIndex = defaultIndex == CurrentIndex;
            if (outOfBounds || sameIndex) return;
            
            // scary while loop D:
            while (CurrentIndex != defaultIndex)
            {
                if (CurrentIndex > defaultIndex)
                    OnNavigateLeft();
                
                if (CurrentIndex < defaultIndex)
                    OnNavigateRight();

                await Awaitable.WaitForSecondsAsync(0.02f);
            }
        }

        private void ToggleRevertButton([NotNull] UIScreen screen)
        {
            revertButton.SetActive(screen.screenType is UIScreen.UIScreenType.Radial && screen.listItems.Count != 0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
            if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
            if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
            if (Input.GetKeyDown(KeyCode.Escape)) OnBack();
            if (Input.GetKeyDown(KeyCode.R)) OnRevert();
        }
    }
}
