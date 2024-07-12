using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
[AddComponentMenu("SaturnGame/Rendering/Guide Lane Renderer")]
public class GuideLaneRenderer : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> laneSegments;
    [SerializeField] private float animationSpeed = 0.008f;
    [SerializeField] private Material material;
    private Material materialInstance;
    
    private static readonly int NoteWidthPropertyID = Shader.PropertyToID("_NoteWidth");
    private static readonly int OpacityPropertyID = Shader.PropertyToID("_TunnelOpacity");
    private static readonly int ComboShinePropertyID = Shader.PropertyToID("_ComboShine");
    private static readonly int LaneTypePropertyID = Shader.PropertyToID("_LaneType");

    private void Awake()
    {
        materialInstance = new(material);
        
        foreach (MeshRenderer meshRenderer in laneSegments)
        {
            meshRenderer.material = materialInstance;
            meshRenderer.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Applies Material settings for different appearances. <br />
    /// Parameters should be from user preferences by default.
    /// </summary>
    /// <param name="noteWidth">Note width from 1 - 5</param>
    /// <param name="opacity">Opacity of Guide Lane</param>
    /// <param name="laneType">Number of visible lanes from 0 to 6</param>
    public void SetRenderer(int noteWidth, int opacity, int laneType)
    {
        material.SetFloat(NoteWidthPropertyID, noteWidth);
        material.SetFloat(OpacityPropertyID, opacity);
        material.SetFloat(LaneTypePropertyID, laneType);
    }


    /// <summary>
    /// Sets a scrolling shine effect to <c>state</c>. <br />
    /// Used for indicating a combo > 200
    /// </summary>
    public void SetComboShine(bool state)
    {
        material.SetFloat(ComboShinePropertyID, Convert.ToInt32(state));
    }


    /// <summary>
    /// Sets GuideLane Mask with a Mask Note.
    /// </summary>
    /// <param name="maskNote">Mask Note to animate from</param>
    /// <param name="speed">Animation speed multiplier</param>
    public async void SetMask([NotNull] Mask maskNote, float speed = 1)
    {
        // Avoid division by zero as a failsafe.
        float clampedSpeed = Mathf.Max(0.00001f, speed);

        int position = maskNote.Position;
        int size = maskNote.Size;
        bool state = maskNote.Add;
        Mask.MaskDirection direction = maskNote.Direction;

        switch (direction)
        {
            case Mask.MaskDirection.Clockwise:
            {
                await AnimateClockwise(position, size, state, clampedSpeed);
                break;
            }
            case Mask.MaskDirection.Counterclockwise:
            {
                await AnimateCounterclockwise(position, size, state, clampedSpeed);
                break;
            }
            case Mask.MaskDirection.Center:
            {
                await AnimateCenter(position, size, state, clampedSpeed);
                break;
            }
            case Mask.MaskDirection.None:
            {
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }


    private async Awaitable AnimateClockwise(int position, int size, bool state, float speed)
    {
        for (int i = 0; i < size; i++)
        {
            laneSegments[(position + size - i + 59) % 60].gameObject.SetActive(state);
            await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
        }
    }

    private async Awaitable AnimateCounterclockwise(int position, int size, bool state, float speed)
    {
        for (int i = 0; i < size; i++)
        {
            laneSegments[(i + position + 60) % 60].gameObject.SetActive(state);
            await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
        }
    }

    private async Awaitable AnimateCenter(int position, int size, bool state, float speed)
    {
        float halfSize = size * 0.5f;
        int floor = Mathf.FloorToInt(halfSize);
        int steps = Mathf.CeilToInt(halfSize);
        int centerClockwise = position + floor;
        int centerCounterclockwise = size % 2 != 0 ? centerClockwise : centerClockwise + 1;
        int offset = size % 2 != 0 ? 60 : 59;

        for (int i = 0; i < steps; i++)
        {
            laneSegments[(centerClockwise - i + offset) % 60].gameObject.SetActive(state);
            laneSegments[(centerCounterclockwise + i + offset) % 60].gameObject.SetActive(state);
            await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
        }
    }
}
}