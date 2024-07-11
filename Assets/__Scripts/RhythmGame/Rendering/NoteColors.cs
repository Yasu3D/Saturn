using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.Rendering
{
public static class NoteColors
{
    /// <summary>
    /// Returns user-selected colors depending on Note
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public static Color GetColor(Note note) => GetColor(GetColorID(note));


    /// <summary>
    /// Returns colors from their corresponding ID
    /// </summary>
    /// <param name="colorID"></param>
    /// <returns></returns>
    public static Color GetColor(int colorID)
    {
        return colorID switch
        {
            0  => new(1.000000f, 0.035601f, 0.863157f, 1.000000f), // Light Magenta
            1  => new(1.000000f, 0.775822f, 0.056128f, 1.000000f), // Light Yellow
            2  => new(1.000000f, 0.323143f, 0.000000f, 1.000000f), // Orange
            3  => new(0.013702f, 0.651406f, 0.008568f, 1.000000f), // Lime
            4  => new(0.730461f, 0.005182f, 0.005182f, 1.000000f), // Red
            5  => new(0.009134f, 0.417885f, 1.000000f, 1.000000f), // Sky Blue
            6  => new(0.327778f, 0.258183f, 0.000000f, 1.000000f), // Dark Yellow
            7  => new(1.000000f, 0.070360f, 0.000000f, 1.000000f), // Light Red
            8  => new(1.000000f, 0.879622f, 0.000000f, 1.000000f), // Yellow
            9  => new(0.035601f, 0.356400f, 0.147027f, 1.000000f), // Pure Green
            10 => new(0.000000f, 0.052861f, 1.000000f, 1.000000f), // Bright Blue
            11 => new(0.132868f, 0.603827f, 1.000000f, 1.000000f), // Light Blue
            12 => new(0.577580f, 0.577580f, 0.597202f, 1.000000f), // Light Gray
            _  => new(0.000000f, 0.000000f, 0.000000f, 1.000000f), // Black
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
                id = settings.NoteColorIDTouch;
                break;
            }
            case ChainNote:
            {
                id = settings.NoteColorIDChain;
                break;
            }
            case SwipeNote { Direction: SwipeNote.SwipeDirection.Clockwise }:
            {
                id = settings.InvertSlideColor != 0
                    ? settings.NoteColorIDSwipeCounterclockwise
                    : settings.NoteColorIDSwipeClockwise;
                break;
            }
            case SwipeNote { Direction: SwipeNote.SwipeDirection.Counterclockwise }:
            {
                id = settings.InvertSlideColor != 0
                    ? settings.NoteColorIDSwipeClockwise
                    : settings.NoteColorIDSwipeCounterclockwise;
                break;
            }
            case SnapNote { Direction: SnapNote.SnapDirection.Forward }:
            {
                id = settings.NoteColorIDSnapForward;
                break;
            }
            case SnapNote { Direction: SnapNote.SnapDirection.Backward }:
            {
                id = settings.NoteColorIDSnapBackward;
                break;
            }
            case HoldNote:
            case HoldSegment:
            {
                id = settings.NoteColorIDHold;
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