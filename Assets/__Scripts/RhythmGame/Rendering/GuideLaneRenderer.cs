using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame.Rendering
{
    public class GuideLaneRenderer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> laneSegments;
        [SerializeField] private float animationSpeed = 0.008f;
        [SerializeField] private Material material;

        /// <summary>
        /// Applies Material settings for different apperances. <br />
        /// Parameters should be from user preferences by default.
        /// </summary>
        /// <param name="noteWidth">Note width from 1 - 5</param>
        /// <param name="opacity">Opacity of Guide Lane</param>
        /// <param name="laneType">Numer of visible lanes from 0 - 6</param>
        public void SetRendererProperties(int noteWidth, int opacity, int laneType)
        {
            material.SetFloat("_NoteWidth", noteWidth);
            material.SetFloat("_Opacity", opacity * 0.2f); // remap from 0-5 to 0-1

            string keyword;
            switch (laneType)
            {
                case 0:
                    keyword = "_NONE";
                    break;
                case 1:
                    keyword = "_A";
                    break;
                case 2:
                    keyword = "_B";
                    break;
                case 3:
                    keyword = "_C";
                    break;
                case 4:
                    keyword = "_D";
                    break;
                case 5:
                    keyword = "_E";
                    break;
                case 6:
                    keyword = "_F";
                    break;
                default:
                    keyword = "";
                    break;
            }

            material.DisableKeyword("_LANETYPE_A");
            material.EnableKeyword("_LANETYPE" + keyword);
        }


        /// <summary>
        /// Sets a scrolling shine effect to <c>state</c>. <br />
        /// Used for indicating a combo > 200
        /// </summary>
        public void SetComboShine(bool state)
        {
            material.SetFloat("_ComboShine", Convert.ToInt32(state));
        }


        /// <summary>
        /// Sets GuideLane Mask with a Mask Note.
        /// </summary>
        /// <param name="maskNote">Mask Note to animate from</param>
        /// <param name="speed">Animation speed multiplier</param>
        public async void SetMask(Note maskNote, float speed = 1)
        {
            if (maskNote.NoteType is not (ObjectEnums.NoteType.MaskAdd or ObjectEnums.NoteType.MaskRemove))
                return;

            // Avoid division by zero as a failsafe.
            float clampedSpeed = Mathf.Max(0.00001f, speed);

            int position = maskNote.Position;
            int size = maskNote.Size;
            bool state = maskNote.NoteType is ObjectEnums.NoteType.MaskAdd;
            ObjectEnums.MaskDirection direction = maskNote.MaskDirection;

            switch (direction)
            {
                case ObjectEnums.MaskDirection.Clockwise:
                    await AnimateClockwise(position, size, state, clampedSpeed);
                    break;
                case ObjectEnums.MaskDirection.Counterclockwise:
                    await AnimateCounterclockwise(position, size, state, clampedSpeed);
                    break;
                case ObjectEnums.MaskDirection.Center:
                    await AnimateCenter(position, size, state, clampedSpeed);
                    break;
            }
        }


        private async Task AnimateClockwise(int position, int size, bool state, float speed)
        {
            for (int i = 0; i < size; i++)
            {
                laneSegments[(position + size - i + 59) % 60].SetActive(state);
                await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
            }
        }

        private async Task AnimateCounterclockwise(int position, int size, bool state, float speed)
        {
            for (int i = 0; i < size; i++)
            {
                laneSegments[(i + position + 60) % 60].SetActive(state);
                await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
            }

        }

        private async Task AnimateCenter(int position, int size, bool state, float speed)
        {
            float halfSize = size * 0.5f;
            int floor = Mathf.FloorToInt(halfSize);
            int steps = Mathf.CeilToInt(halfSize);
            int centerClockwise = position + floor;
            int centerCounterclockwise = size % 2 != 0 ? centerClockwise : centerClockwise + 1;
            int offset = size % 2 != 0 ? 60 : 59;

            for (int i = 0; i < steps; i++)
            {
                laneSegments[(centerClockwise - i + offset) % 60].SetActive(state);
                laneSegments[(centerCounterclockwise + i + offset) % 60].SetActive(state);
                await Awaitable.WaitForSecondsAsync(animationSpeed / speed);
            }
        }
    }
}
