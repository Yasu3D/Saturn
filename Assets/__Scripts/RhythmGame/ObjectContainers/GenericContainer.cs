using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [AddComponentMenu("SaturnGame/Rendering/Containers/Generic Container")]
    public class GenericContainer : MonoBehaviour
    {
        public PositionedChartElement note;
        new public GenericRenderer renderer;
        public bool reverse = false;
    }
}
