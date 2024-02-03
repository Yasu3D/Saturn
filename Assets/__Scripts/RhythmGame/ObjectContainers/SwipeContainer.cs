using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Swipe Container")]
    public class SwipeContainer : MonoBehaviour
    {
        public SwipeNote note;
        new public SwipeRenderer renderer;
        public bool reverse = false;
    }
}
