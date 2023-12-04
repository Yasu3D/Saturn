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

        [SerializeField] private Note test;

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

        public void SetComboShine(bool state)
        {
            material.SetFloat("_ComboShine", Convert.ToInt32(state));
        }

        /// <summary>
        /// Sets GuideLane Mask with a Mask Note.
        /// </summary>
        /// <param name="maskNote"></param>
        public async void SetMask(Note maskNote)
        {
            if (maskNote.NoteType is not (ObjectEnums.NoteType.MaskAdd or ObjectEnums.NoteType.MaskRemove))
                return;

            int position = maskNote.Position;
            int size = maskNote.Size;
            bool state = maskNote.NoteType is ObjectEnums.NoteType.MaskAdd;
            ObjectEnums.MaskDirection direction = maskNote.MaskDirection;

            switch (direction)
            {
                case ObjectEnums.MaskDirection.Clockwise:
                    await AnimateClockwise(position, size, state);
                    break;
                case ObjectEnums.MaskDirection.Counterclockwise:
                    await AnimateCounterclockwise(position, size, state);
                    break;
                case ObjectEnums.MaskDirection.Center:
                    await AnimateCenter(position, size, state);
                    break;
            }
        }

        private async Task AnimateClockwise(int position, int size, bool state)
        {
            for (int i = 0; i < size; i++)
            {
                laneSegments[(position + size - i + 59) % 60].SetActive(state);
                await Awaitable.WaitForSecondsAsync(animationSpeed);
            }
        }

        private async Task AnimateCounterclockwise(int position, int size, bool state)
        {
            for (int i = 0; i < size; i++)
            {
                laneSegments[(i + position + 60) % 60].SetActive(state);
                await Awaitable.WaitForSecondsAsync(animationSpeed);
            }

        }

        private async Task AnimateCenter(int position, int size, bool state)
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
                await Awaitable.WaitForSecondsAsync(animationSpeed);
            }
        }
    
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
                SetMask(test);
        }
    }
}
