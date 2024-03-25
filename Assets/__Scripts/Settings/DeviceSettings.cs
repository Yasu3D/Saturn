using UnityEngine;

namespace SaturnGame.Settings
{
[System.Serializable]
public class DeviceSettings
{
    public DisplaySettings DisplaySettings = new();
}

[System.Serializable]
public class DisplaySettings
{
    [Range(0, 100)] public int ViewRectPosition = 56;
    [Range(50, 100)] public int ViewRectScale = 100;
    public int TargetFramerate = 120;
}
}