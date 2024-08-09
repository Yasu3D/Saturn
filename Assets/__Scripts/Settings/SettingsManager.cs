namespace SaturnGame.Settings
{
public class SettingsManager : PersistentSingleton<SettingsManager>
{
    public PlayerSettings PlayerSettings;
    public DeviceSettings DeviceSettings;

    protected override void Awake()
    {
        base.Awake();

        PlayerSettings = PlayerSettings.Load();
    }
}
}
