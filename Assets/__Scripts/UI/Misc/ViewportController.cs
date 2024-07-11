using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SaturnGame.UI
{
public class ViewportController : UIBehaviour
{
    [Range(-1,1)] public float Offset = 0;
    [Range(0,1)] public float Scale = 1;

    [SerializeField] private RectTransform thisRectTransform;
    [SerializeField] private RectTransform childRectTransform;

    protected override void Start()
    {
        base.Start();
        OnDisplaySettingsChanged();
    }
    
    protected override void OnRectTransformDimensionsChange()
    {
        UpdateRect();
    }

    protected override void OnValidate()
    {
        UpdateRect();
    }

    private void OnDisplaySettingsChanged()
    {
        Offset = SettingsManager.Instance.DeviceSettings.DisplaySettings.ViewRectPosition * 0.01f;
        Scale = SettingsManager.Instance.DeviceSettings.DisplaySettings.ViewRectScale * 0.01f;
        UpdateRect();
    }

    public void UpdateRect()
    {
        if (thisRectTransform == null || childRectTransform == null) return;

        float xDelta = thisRectTransform.sizeDelta.x;
        float yDelta = thisRectTransform.sizeDelta.y;
        
        childRectTransform.offsetMin = new(xDelta * Offset * 0.5f, yDelta * Offset * 0.5f);
        childRectTransform.offsetMax = new(xDelta * Offset * 0.5f, yDelta * Offset * 0.5f);
        childRectTransform.localScale = new(Scale, Scale, Scale);
    }
}
}
