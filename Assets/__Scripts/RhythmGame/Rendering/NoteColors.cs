using System;
using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.Rendering
{
    public static class NoteColors
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
            return colorID switch
            {
                0 =>  (new Color(1.000f, 0.220f, 0.930f), 0.48f), // Light Magenta
                1 =>  (new Color(1.000f, 0.910f, 0.420f), 0.50f), // Light Yellow
                2 =>  (new Color(1.000f, 0.600f, 0.000f), 0.25f), // Orange
                3 =>  (new Color(0.140f, 0.820f, 0.110f), 0.33f), // Lime
                4 =>  (new Color(0.870f, 0.090f, 0.090f), 0.45f), // Red
                5 =>  (new Color(0.120f, 0.670f, 1.000f), 0.30f), // Sky Blue
                6 =>  (new Color(0.600f, 0.540f, 0.000f), 0.20f), // Dark Yellow
                7 =>  (new Color(1.000f, 0.300f, 0.000f), 0.40f), // Light Red
                8 =>  (new Color(1.000f, 0.940f, 0.000f), 0.30f), // Yellow
                9 =>  (new Color(0.220f, 0.620f, 0.420f), 0.20f), // Pure Green
                10 => (new Color(0.000f, 0.260f, 1.000f), 0.30f), // Bright Blue
                11 => (new Color(0.400f, 0.800f, 1.000f), 0.30f), // Light Blue
                12 => (new Color(0.780f, 0.780f, 0.780f), 0.30f), // Light Gray  
                _ =>  (new Color(0.000f, 0.000f, 0.000f), 0.30f)
            };
        }


        /// <summary>
        /// Returns colors adjusted for the swipe shader from their corresponding ID
        /// </summary>
        /// <param name="colorID"></param>
        /// <returns></returns>
        public static Color GetSwipeColor(int colorID)
        {
            return colorID switch
            {
                0 =>  new Color(0.850f, 0.105f, 0.737f), // Light Magenta
                1 =>  new Color(0.850f, 0.662f, 0.129f), // Light Yellow
                2 =>  new Color(0.850f, 0.329f, 0.000f), // Orange
                3 =>  new Color(0.070f, 0.564f, 0.043f), // Lime
                4 =>  new Color(0.631f, 0.000f, 0.000f), // Red
                5 =>  new Color(0.000f, 0.388f, 0.855f), // Sky Blue
                6 =>  new Color(0.325f, 0.278f, 0.000f), // Dark Yellow
                7 =>  new Color(0.855f, 0.133f, 0.000f), // Light Red
                8 =>  new Color(0.855f, 0.752f, 0.000f), // Yellow
                9 =>  new Color(0.086f, 0.345f, 0.196f), // Pure Green
                10 => new Color(0.000f, 0.113f, 0.855f), // Bright Blue
                11 => new Color(0.180f, 0.529f, 0.855f), // Light Blue 
                12 => new Color(0.510f, 0.509f, 0.525f), // Light Gray  
                _ =>  new Color(0.000f, 0.000f, 0.000f)
            };
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
                {
                    id = settings.NoteColorID_Touch;
                    break;
                }
                case ChainNote:
                {
                    id = settings.NoteColorID_Chain;
                    break;
                }
                case SwipeNote { Direction: SwipeNote.SwipeDirection.Clockwise }:
                {
                    if (settings.InvertSlideColor != 0) id = settings.NoteColorID_SwipeCounterclockwise;
                    else id = settings.NoteColorID_SwipeClockwise;
                    break;
                }
                case SwipeNote { Direction: SwipeNote.SwipeDirection.Counterclockwise }:
                {
                    if (settings.InvertSlideColor != 0) id = settings.NoteColorID_SwipeClockwise;
                    else id = settings.NoteColorID_SwipeCounterclockwise;
                    break;
                }
                case SnapNote { Direction: SnapNote.SnapDirection.Forward }:
                {
                    id = settings.NoteColorID_SnapForward;
                    break;
                }
                case SnapNote { Direction: SnapNote.SnapDirection.Backward }:
                {
                    id = settings.NoteColorID_SnapBackward;
                    break;
                }
                case HoldNote:
                case HoldSegment:
                {
                    id = settings.NoteColorID_Hold;
                    break;
                }

                default:
                {
                    id = 0;
                    break;
                }
            }
            
            return id;
        }
    }
}