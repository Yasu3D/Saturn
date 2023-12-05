using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Containers/Note Container")]
    public class NoteContainer : MonoBehaviour
    {
        public Note note;
        public NoteRenderer noteRenderer;
        public SwipeRenderer swipeRenderer = null;
        public SnapRenderer snapRenderer = null;
    }
}
