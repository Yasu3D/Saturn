using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("SaturnGame/Rendering/Swipe Renderer")]
    public class SwipeRenderer : AbstractPositionedChartElementRenderer<SwipeNote>
    {
        [SerializeField] private Material materialTemplate;
        private Material materialInstance;

        public Color Color { get; private set; }
        public string Direction { get; private set; } = "_COUNTERCLOCKWISE";

        void Awake()
        {
            materialInstance = new(materialTemplate);
        }

        public override void SetRenderer(SwipeNote note)
        {
            Size = note.Size;
            Position = note.Position;

            int colorID = NoteColors.GetColorID(note);
            Color = NoteColors.GetSwipeColor(colorID);

            Direction = note.Direction is SwipeNote.SwipeDirection.Counterclockwise ? "_COUNTERCLOCKWISE" : "_CLOCKWISE";

            if (materialInstance.HasColor("_NoteColor"))
                materialInstance.SetColor("_NoteColor", Color);

            materialInstance.DisableKeyword("_DIRECTION_COUNTERCLOCKWISE");
            materialInstance.EnableKeyword("_DIRECTION" + Direction);

            meshFilter.mesh = meshes[Size - 1];
            meshRenderer.material = materialInstance;

            transform.eulerAngles = new Vector3 (0, 0, Position * -6);
        }
    }
}
