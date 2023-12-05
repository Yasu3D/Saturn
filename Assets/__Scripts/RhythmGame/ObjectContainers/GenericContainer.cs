using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Generic Container")]
    public class GenericContainer : MonoBehaviour
    {
        public Note note;
        new public GenericRenderer renderer;
    }
}
