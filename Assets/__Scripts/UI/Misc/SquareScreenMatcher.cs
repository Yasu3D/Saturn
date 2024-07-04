using UnityEngine;
using UnityEngine.UI;

namespace SaturnGame
{
    public class SquareScreenMatcher : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;
        
        //private void OnEnable() => EventManager.AddListener("OnResolutionChange", OnResolutionChange);
        //private void OnDisable() => EventManager.RemoveListener("OnResolutionChange", OnResolutionChange);

        private void OnResolutionChange()
        {
            // aspectRatio > 1 = Landscape
            // aspectRatio < 1 = Portrait
            float aspectRatio = (float)Screen.width / Screen.height;
            canvasScaler.matchWidthOrHeight = aspectRatio > 1 ? 1 : 0;
        }
    }
}
