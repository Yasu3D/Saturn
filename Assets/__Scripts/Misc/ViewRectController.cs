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

    private static readonly int RadiusNameID = Shader.PropertyToID("_Radius");

    private void Awake()
    {
        Screen.fullScreen = false;
        List<DisplayInfo> displays = new();
        Screen.GetDisplayLayout(displays);
        foreach (DisplayInfo display in displays.Where(display => display.height > display.width))
        {
            Screen.MoveMainWindowTo(display, Vector2Int.zero);
            break;
        }

        Resolution resolution = Screen.resolutions
            .OrderByDescending(r => r.height)
            .ThenByDescending(r => r.width)
            .First();
        Screen.SetResolution(resolution.width, resolution.height, true);
        OnUpdateViewRect();
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateViewRect", OnUpdateViewRect);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateViewRect", OnUpdateViewRect);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        OnUpdateViewRect();
    }

    private void OnUpdateViewRect()
    {
        DisplaySettings settings = SettingsManager.Instance.DeviceSettings.DisplaySettings;
        float scale = settings.ViewRectScale * 0.01f;
        mask.material.SetFloat(RadiusNameID, Mathf.Lerp(rBounds.x, rBounds.y, scale));
    }
}