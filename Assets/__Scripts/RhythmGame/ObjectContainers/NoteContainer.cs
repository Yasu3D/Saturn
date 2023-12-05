using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Note Container")]
    public class NoteContainer : MonoBehaviour
    {
        public Note note;
        new public NoteRenderer renderer;
    }
}
