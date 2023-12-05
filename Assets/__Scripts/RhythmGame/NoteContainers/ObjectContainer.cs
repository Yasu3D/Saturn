using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Containers/Object Container")]
    public class ObjectContainer : MonoBehaviour
    {
        public Note note;
        public ObjectRenderer objectRenderer;
    }
}
