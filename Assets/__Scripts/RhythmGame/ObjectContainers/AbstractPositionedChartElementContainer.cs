using SaturnGame.Rendering;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    //[AddComponentMenu("SaturnGame/Rendering/Containers/Generic Container")]
    public abstract class AbstractPositionedChartElementContainer<TContained, TRenderer> : MonoBehaviour where TContained : PositionedChartElement where TRenderer : AbstractPositionedChartElementRenderer<TContained>
    {
        public TContained note;
        public new TRenderer renderer;
        public bool reverse = false;
    }
}
