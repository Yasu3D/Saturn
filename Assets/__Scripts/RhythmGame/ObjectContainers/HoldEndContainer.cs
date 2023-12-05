using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Hold End Container")]
    public class HoldEndContainer : MonoBehaviour
    {
        public Note note;
        new public HoldEndRenderer renderer;
    }
}
