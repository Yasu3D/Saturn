using SaturnGame.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewRectController : MonoBehaviour
{
    [SerializeField] private Image mask;

    [SerializeField] private Vector2 rBounds = new(0, 0.3237f);
    [SerializeField] private Vector2 oBounds = new(0, 840);

    void Awake()
    {
        Screen.fullScreen = false;
        List<DisplayInfo> displays = new();
        Screen.GetDisplayLayout(displays);
        foreach (DisplayInfo display in displays)
        {
            if (display.height > display.width)
            {
                Screen.MoveMainWindowTo(display, Vector2Int.zero);
                break;
            }
        }
        var resolution = Screen.resolutions
            .OrderByDescending(r => r.height)
            .ThenByDescending(r => r.width)
            .First();
        Screen.SetResolution(resolution.width, resolution.height, true);
        OnUpdateViewRect();
    }

    void OnEnable()
    {
        EventManager.AddListener("UpdateViewRect", OnUpdateViewRect);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        EventManager.RemoveListener("UpdateViewRect", OnUpdateViewRect);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        OnUpdateViewRect();
    }

    public void OnUpdateViewRect()
    {
        DisplaySettings settings = SettingsManager.Instance.DeviceSettings.DisplaySettings;

        float currentAspect = (float)Screen.width / Screen.height;
        float pos = (settings.ViewRectPosition - 50) * 0.01f; // remap from [0<>100] to [-0.5<>+0.5]
        float scale = settings.ViewRectScale * 0.01f;

        SetViewRect(currentAspect, pos, scale);
        SetMask(currentAspect, pos, scale);
    }

    public void SetViewRect(float currentAspect, float pos, float scale)
    {
        if (Camera.main == null) return;

        float x, y, w, h;
        if (currentAspect < 1.0f)
        {
            // Portrait
            x = 0.5f * (1 - scale);
            y = (1 - currentAspect) * 0.5f + pos * (1 - currentAspect);
            w = scale;
            h = currentAspect * scale;

            y += h * ((0.5f / scale) - 0.5f);
        }
        else
        {
            // Landscape
            x = (1 - (1 / currentAspect)) * 0.5f + pos * (1 - (1 / currentAspect));
            y = 0.5f * (1 - scale);
            w = 1 / currentAspect * scale;
            h = scale;

            x += w * ((0.5f / scale) - 0.5f);
        }

        Camera.main.rect = new(x,y,w,h);
    }

    public void SetMask(float currentAspect, float pos, float scale)
    {
        float radius = Mathf.Lerp(rBounds.x, rBounds.y, scale);
        float offset = Mathf.LerpUnclamped(oBounds.x, oBounds.y, pos);

        if (currentAspect < 1.0f)
        {
            // Portrait
            mask.rectTransform.localPosition = new(0, offset, 0);
        }
        else
        {
            // Landscape
            mask.rectTransform.localPosition = new(offset, 0, 0);
        }

        mask.material.SetFloat("_Radius", radius);
    }
}
