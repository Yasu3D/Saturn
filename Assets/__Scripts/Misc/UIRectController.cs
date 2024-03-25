using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.Serialization;

public class UIRectController : MonoBehaviour
{
    [FormerlySerializedAs("UI")] [SerializeField] private RectTransform ui;

    [SerializeField] private Vector2 oBounds = new(0, 840);

    private void Start()
    {
        OnUpdateViewRect();
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateViewRect", OnUpdateViewRect);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateViewRect", OnUpdateViewRect);
    }

    private void OnUpdateViewRect()
    {
        DisplaySettings settings = SettingsManager.Instance.DeviceSettings.DisplaySettings;
        SetViewRect(settings.ViewRectPosition, settings.ViewRectScale);
    }

    private void SetViewRect(int position, int scale)
    {
        float currentAspect = (float)Screen.width / Screen.height;

        float p = (position - 50) * 0.01f; // remap from [0<>100] to [-0.5<>+0.5]
        float s = scale * 0.01f;

        float o = Mathf.LerpUnclamped(oBounds.x, oBounds.y, p);

        ui.localPosition = currentAspect < 1.0f ?
            // Portrait
            new Vector3(0, o, 0) :
            // Landscape
            new Vector3(o, 0, 0);

        ui.localScale = new Vector3(s, s, s);
    }
}
