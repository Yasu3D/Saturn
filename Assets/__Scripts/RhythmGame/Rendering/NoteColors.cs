using System;
using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.Rendering
{
    public class NoteColors
    {
        /// <summary>
        /// Returns user-selected colors and brightness values depending on Note
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static (Color color, float subtract) GetColor(Note note)
        {
            int id = GetColorID(note);

            return GetColor(id);
        }


        /// <summary>
        /// Returns colors and brightness from their corresponding ID
        /// </summary>
        /// <param name="colorID"></param>
        /// <returns></returns>
        public static (Color color, float subtract) GetColor(int colorID)
        {
            switch (colorID)
            {
                case 0:
                    // Light Magenta
                    return (new Color (1.0f, 0.22f, 0.93f), 0.48f);
                
                case 1:
                    // Light Yellow
                    return (new Color (1.0f, 0.91f, 0.42f), 0.5f);
                
                case 2:
                    // Orange
                    return (new Color (1.0f, 0.6f, 0.0f), 0.25f);
                
                case 3:
                    // Lime
                    return (new Color (0.14f, 0.82f, 0.11f), 0.33f);
                
                case 4:
                    // Red
                    return (new Color (0.87f, 0.09f, 0.09f), 0.45f);

                case 5:
                    // Sky Blue
                    return (new Color (0.12f, 0.67f, 1.0f), 0.3f);
                
                case 6:
                    // Dark Yellow
                    return (new Color (0.6f, 0.54f, 0.0f), 0.2f);
                
                case 7:
                    // Light Red
                    return (new Color (1.0f, 0.3f, 0.0f), 0.4f);
                
                case 8:
                    // Yellow
                    return (new Color (1.0f, 0.94f, 0.0f), 0.3f);
                
                case 9:
                    // Pure Green
                    return (new Color (0.22f, 0.62f, 0.42f), 0.2f);
                
                case 10:
                    // Bright Blue
                    return (new Color (0.0f, 0.26f, 1.0f), 0.3f);

                case 11:
                    // Light Blue
                    return (new Color (0.4f, 0.8f, 1.0f), 0.3f);
                
                case 12:
                    // Light Gray  
                    return (new Color (0.78f, 0.78f, 0.78f), 0.3f);

                default:
                    return (new Color (0,0,0), 0.3f);
            }
        }


        /// <summary>
        /// Returns colors adjusted for the swipe shader from their corresponding ID
        /// </summary>
        /// <param name="colorID"></param>
        /// <returns></returns>
        public static Color GetSwipeColor(int colorID)
        {
            switch (colorID)
            {
                case 0:
                    // Light Magenta
                    return new Color (0.850f, 0.105f, 0.737f);
                
                case 1:
                    // Light Yellow
                    return new Color (0.850f, 0.662f, 0.129f);
                
                case 2:
                    // Orange
                    return new Color (0.850f, 0.329f, 0.000f);
                
                case 3:
                    // Lime
                    return new Color (0.070f, 0.564f, 0.043f);
                
                case 4:
                    // Red
                    return new Color (0.631f, 0.000f, 0.000f);

                case 5:
                    // Sky Blue
                    return new Color (0.000f, 0.388f, 0.855f);
                
                case 6:
                    // Dark Yellow
                    return new Color (0.325f, 0.278f, 0.000f);
                
                case 7:
                    // Light Red
                    return new Color (0.855f, 0.133f, 0.000f);
                
                case 8:
                    // Yellow
                    return new Color (0.855f, 0.752f, 0.000f);
                
                case 9:
                    // Pure Green
                    return new Color (0.086f, 0.345f, 0.196f);
                
                case 10:
                    // Bright Blue
                    return new Color (0.000f, 0.113f, 0.855f);

                case 11:
                    // Light Blue
                    return new Color (0.180f, 0.529f, 0.855f);
                
                case 12:
                    // Light Gray  
                    return new Color (0.510f, 0.509f, 0.525f);

                default:
                    return new Color (0,0,0);
            }
        }


        /// <summary>
        /// Returns color ID from user settings.
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static int GetColorID(ChartElement note)
        {
            int id;
            DesignSettings settings = SettingsManager.Instance.PlayerSettings.DesignSettings;

            switch (note)
            {
                case TouchNote:
                    id = settings.NoteColorID_Touch;
                    break;
                case ChainNote:
                    id = settings.NoteColorID_Chain;
                    break;
                case SwipeNote { Direction: SwipeNote.SwipeDirection.Clockwise }:
                    if (settings.InvertSlideColor != 0) id = settings.NoteColorID_SwipeCounterclockwise;
                    else id = settings.NoteColorID_SwipeClockwise;
                    break;
                case SwipeNote { Direction: SwipeNote.SwipeDirection.Counterclockwise }:
                    if (settings.InvertSlideColor != 0) id = settings.NoteColorID_SwipeClockwise;
                    else id = settings.NoteColorID_SwipeCounterclockwise;
                    break;
                case SnapNote { Direction: SnapNote.SnapDirection.Forward }:
                    id = settings.NoteColorID_SnapForward;
                    break;
                case SnapNote { Direction: SnapNote.SnapDirection.Backward }:
                    id = settings.NoteColorID_SnapBackward;
                    break;
                case HoldNote:
                case HoldSegment:
                    id = settings.NoteColorID_Hold;
                    break;

                default:
                    id = 0;
                    break;
            }
            
            return id;
        }
    }
}