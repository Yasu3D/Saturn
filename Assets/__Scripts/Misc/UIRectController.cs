using System.Collections;
using System.Collections.Generic;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class UIRectController : MonoBehaviour
{
    [SerializeField] private RectTransform UI;

    [SerializeField] private Vector2 oBounds = new(0, 840);

    void Start()
    {
        OnUpdateViewRect();
    }
    
    void OnEnable()
    {
        EventManager.AddListener("UpdateViewRect", OnUpdateViewRect);
    }

    void OnDisable()
    {
        EventManager.RemoveListener("UpdateViewRect", OnUpdateViewRect);
    }

    public void OnUpdateViewRect()
    {
        DisplaySettings settings = SettingsManager.Instance.DeviceSettings.DisplaySettings;
        SetViewRect(settings.ViewRectPosition, settings.ViewRectScale);
    }

    public void SetViewRect(int position, int scale)
    {
        float currentAspect = (float)Screen.width / Screen.height;

        float p = (position - 50) * 0.01f; // remap from [0<>100] to [-0.5<>+0.5]
        float s = scale * 0.01f;

        float o = Mathf.LerpUnclamped(oBounds.x, oBounds.y, p);
        
        if (currentAspect < 1.0f)
        {
            // Portrait
            UI.localPosition = new(0, o, 0);
        }
        else
        {
            // Landscape
            UI.localPosition = new(o, 0, 0);
        }

        UI.localScale = new(s, s, s);
    }
}
