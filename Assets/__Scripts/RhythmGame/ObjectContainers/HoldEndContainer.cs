using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Hold End Container")]
    public class HoldEndContainer : MonoBehaviour
    {
        public HoldSegment holdEnd;
        new public HoldEndRenderer renderer;
        public bool reverse = false;
    }
}
