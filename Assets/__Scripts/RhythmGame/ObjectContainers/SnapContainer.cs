using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Snap Container")]
    public class SnapContainer : MonoBehaviour
    {
        public Note note;
        new public SnapRenderer renderer;
    }
}
