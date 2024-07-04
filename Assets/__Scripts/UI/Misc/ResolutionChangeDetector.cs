using UnityEngine;

namespace SaturnGame.UI
{
public class ResolutionChangeDetector : MonoBehaviour
{
    private static int width;
    private static int height;
    
    private void Update()
    {
        if (width != Screen.width || height != Screen.height)
        {
            EventManager.InvokeEvent("OnResolutionChange");
            
            width = Screen.width;
            height = Screen.height;
        }
    }
}
}
