using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Settings
{
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        public PlayerSettings PlayerSettings;
        public DeviceSettings DeviceSettings;
    }
}
