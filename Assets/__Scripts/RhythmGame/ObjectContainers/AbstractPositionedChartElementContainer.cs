using SaturnGame.Rendering;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.RhythmGame
{
//[AddComponentMenu("SaturnGame/Rendering/Containers/Generic Container")]
public abstract class AbstractPositionedChartElementContainer<TContained, TRenderer> : MonoBehaviour
    where TContained : PositionedChartElement
    where TRenderer : AbstractPositionedChartElementRenderer<TContained>
{
    public TContained Note;
    [FormerlySerializedAs("renderer")] public TRenderer Renderer;
    public bool Reverse;
}
}