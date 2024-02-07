using System.Collections;
using System.Collections.Generic;
using SaturnGame.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewRectController : StaticInstance<ViewRectController>
{
    [SerializeField] private Image mask;
    [SerializeField] public TextMeshProUGUI DebugText;

    [SerializeField] private Vector2 rBounds = new(0, 0.3237f);
    [SerializeField] private Vector2 oBounds = new(0, 840);

    protected override void Awake()
    {
        base.Awake();
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
        PositionDebugText(currentAspect, pos, scale);
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

    public void PositionDebugText(float currentAspect, float pos, float scale)
    {
        float offset = Mathf.LerpUnclamped(oBounds.x, oBounds.y, pos);

        if (currentAspect < 1.0f)
        {
            // Portrait

            // Pivot bottom left
            DebugText.rectTransform.pivot = new(0, 0);

            // Text align bottom
            DebugText.verticalAlignment = VerticalAlignmentOptions.Bottom;

            // Anchor to top left of square viewport
            float radiusPos = scale * Screen.width / 2f;
            DebugText.rectTransform.localPosition = new(-radiusPos, radiusPos + offset, 0);
        }
        else
        {
            // Landscape

            // Pivot top left
            DebugText.rectTransform.pivot = new(0, 1);

            // Text align top
            DebugText.verticalAlignment = VerticalAlignmentOptions.Top;

            // Anchor to top right of square viewport
            float radiusPos = scale * Screen.height / 2f;
            DebugText.rectTransform.localPosition = new(radiusPos + offset, radiusPos, 0);
        }
    }
}
