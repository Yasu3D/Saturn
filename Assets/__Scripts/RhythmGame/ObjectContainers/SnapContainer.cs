using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Snap Container")]
    public class SnapContainer : MonoBehaviour
    {
        public SnapNote note;
        new public SnapRenderer renderer;
        public bool reverse = false;
    }
}
