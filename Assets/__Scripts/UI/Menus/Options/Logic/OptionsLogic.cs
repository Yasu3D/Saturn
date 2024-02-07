using System.Collections;
using System.Collections.Generic;
using SaturnGame;
using UnityEngine;

public class OptionsLogic : MonoBehaviour
{
    [SerializeField] private OptionsPanelAnimator panelAnimator;

    [SerializeField] private int layerIndex;
    [SerializeField] private int optionIndex;
    private Stack<int> optionIndexMemory = new();

    public void OnConfirm()
    {
        layerIndex++;

        optionIndexMemory.Push(optionIndex);
        optionIndex = 0;
    }

    public void OnBack()
    {
        if (layerIndex <= 0)
        {
            layerIndex = 0;
            return;
        }

        layerIndex--;

        optionIndex = optionIndexMemory.Peek();
        optionIndexMemory.Pop();
    }

    public void OnNavigateLeft()
    {
        if (optionIndex <= 0)
        {
            optionIndex = 0;
            return;
        }
        
        optionIndex--;
        panelAnimator.Anim_ShiftPanels(optionIndex);
    }
    
    public void OnNavigateRight()
    {
        optionIndex++;
        panelAnimator.Anim_ShiftPanels(optionIndex);
    }

    public void OnDefault() {}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) OnNavigateLeft();
        if (Input.GetKeyDown(KeyCode.D)) OnNavigateRight();
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
        if (Input.GetKeyDown(KeyCode.Escape)) OnBack();

        if (Input.GetKeyDown(KeyCode.UpArrow)) panelAnimator.Anim_ShowPanels();
        if (Input.GetKeyDown(KeyCode.DownArrow)) panelAnimator.Anim_HidePanels(optionIndex);
    }
}
