using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("SaturnGame/Rendering/Generic Renderer")]
public class GenericRenderer : AbstractPositionedChartElementRenderer<PositionedChartElement>
{
}
}