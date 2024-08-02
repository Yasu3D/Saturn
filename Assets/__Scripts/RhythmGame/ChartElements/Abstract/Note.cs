using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// Note represents any note type in the chart that needs to be hit and receives a judgement.
/// </summary>
/// Note (pun intended): For hold notes, the "position" is the the position of the start.
[Serializable]
public abstract class Note : PositionedChartElement
{
    // ID must be unique within a given chart.
    public int ID;

    public NoteBonusType BonusType;

    // IsSync is true if this note is a "sync" note, that is,
    // it is at the same time as another note and is highlighted
    public bool IsSync;

    public float? EarliestHitTimeMs;
    public float? LatestHitTimeMs;

    // A note can be in one of the following states:
    // - Not yet hit (Hit and Judged are both false)
    // - Hit (the HitWindows have been evaluated - HitWindowsEvaluated is true, but IsJudged is false)
    // - Judged (a judgement has been assigned - HitWindowsEvaluated and IsJudged are both true)
    // For most note types, a note is Judged as soon as its hit windows are evaluated,
    // so HitWindowsEvaluated == IsJudged at all times. However, for hold notes, the hit windows are evaluated at the
    // beginning of the hold, but the note is not Judged until the end of the hold.
    /// Please make sure to use the right one.

    // If the note has not been judged, this must be null (not None).
    public Judgement? Judgement;

    // IsJudged refers to whether the note has been assigned a judgement.
    public bool IsJudged => Judgement is not null;

    // Refers to whether the HitWindows of a note have been evaluated.
    // This will be true for Misses.
    // Generally this just checks if Judgement is present, but it's overriden for HoldNotes.
    public virtual bool HitWindowsEvaluated => Judgement is not null;

    // HasBeenHit refers to whether the hit windows of the note have been hit.
    // This will never be true for misses.
    // For hold notes, it's possible that a note HasBeenHit but hasn't yet been Judged.
    public virtual bool HasBeenHit => Judgement is not null && Judgement != RhythmGame.Judgement.Miss;

    // For a HoldNote, hit time of start, otherwise the hit time of the note.
    // A null HitTimeMs is possible if the HitWindows were judged as a Miss, or if the note hasn't been hit yet.
    // Note that HitTimeMs may be unreliable for ChainNotes - see ChainNote.cs for details.
    public float? HitTimeMs;

    // The error in ms of this input compared to a perfectly-timed input.
    // e.g. 5ms early will give a value of -5
    // A null TimeErrorMs is possible if the HitWindows were judged as a Miss, or if the note hasn't been hit yet.
    public float? TimeErrorMs => HitTimeMs - TimeMs;

    protected Note(int measure, int tick, int position, int size, int id, NoteBonusType bonusType, bool isSync = false) : base(measure, tick, position, size)
    {
        ID = id;
        BonusType = bonusType;
        IsSync = isSync;
    }

    protected Note()
    {
    }

    public void SetBonusTypeFromNoteID(int noteID)
    {
        switch (noteID)
        {
            case 1:
            case 3:
            case 4:
            case 5:
            case 7:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 16:
            {
                BonusType = NoteBonusType.None;
                break;
            }

            case 2:
            case 6:
            case 8:
            {
                BonusType = NoteBonusType.Bonus;
                break;
            }

            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
            case 26:
            {
                BonusType = NoteBonusType.RNote;
                break;
            }

            default:
            {
                throw new ArgumentException($"Unknown note ID {noteID}", nameof(noteID));
            }
        }
    }

    [NotNull]
    public static Note CreateFromNoteTypeID(int measure, int tick, int noteTypeID, int position, int size, int id)
    {
        Note note;

        switch (noteTypeID)
        {
            case 1:
            case 2:
            case 20:
            {
                note = new TouchNote(measure, tick, position, size, id);
                break;
            }

            case 3:
            case 21:
            {
                note = new SnapNote(measure, tick, position, size, id, SnapNote.SnapDirection.Forward);
                break;
            }

            case 4:
            case 22:
            {
                note = new SnapNote(measure, tick, position, size, id, SnapNote.SnapDirection.Backward);
                break;
            }

            case 5:
            case 6:
            case 23:
            {
                note = SwipeNote.CreateSwipe(measure, tick, position, size, id, SwipeNote.SwipeDirection.Clockwise);
                break;
            }

            case 7:
            case 8:
            case 24:
            {
                note = SwipeNote.CreateSwipe(measure, tick, position, size, id, SwipeNote.SwipeDirection.Counterclockwise);
                break;
            }


            case 16:
            case 26:
            {
                note = new ChainNote(measure, tick, position, size, id);
                break;
            }

            case 9:
            case 25:
            case 10:
            case 11:
            {
                throw new ArgumentException(
                    $"Note type ID {noteTypeID} represents a HoldNote component which is unsupported by this function",
                    nameof(noteTypeID));
            }

            case 12:
            case 13:
            case 14:
            {
                throw new ArgumentException($"Note ID {noteTypeID} does not represent a Note", nameof(noteTypeID));
            }

            default:
            {
                throw new ArgumentException($"Unknown note ID {noteTypeID}", nameof(noteTypeID));
            }
        }

        note.SetBonusTypeFromNoteID(noteTypeID);

        return note;
    }

    public enum NoteBonusType
    {
        None,
        Bonus,
        RNote,
    }

    protected const float FrameMs = 1000f / 60f;

    protected static HitWindow[] BaseHitWindows =
    {
        // Touch note windows from the original game.
        // Note: these are frame-based, so the feel will be different.
        new(-3 * FrameMs, 3 * FrameMs, RhythmGame.Judgement.Marvelous),
        new(-5 * FrameMs, 5 * FrameMs, RhythmGame.Judgement.Great),
        new(-6 * FrameMs, 6 * FrameMs, RhythmGame.Judgement.Good),
        // There is no early or late Miss window.
        // You get a Miss if all windows pass without hitting the note.
    };

    public abstract HitWindow[] HitWindows { get; }

    public HitWindow? ActiveHitWindow(float hitTimeMs)
    {
        float errorMs = hitTimeMs - TimeMs;
        return HitWindows
            .Where(hitWindow => errorMs >= hitWindow.LeftMs && errorMs < hitWindow.RightMs)
            .Select(hitWindow => (HitWindow?)hitWindow)
            .FirstOrDefault();
    }

    public virtual Judgement Hit(float hitTimeMs)
    {
        HitWindow? activeHitWindow = ActiveHitWindow(hitTimeMs);
        if (activeHitWindow is not { Judgement: var judgement })
        {
            throw new ArgumentException($"Hit time {hitTimeMs} is not within any hit window for note at {TimeMs}",
                nameof(hitTimeMs));
        }

        Judgement = judgement;
        HitTimeMs = hitTimeMs;
        return judgement;
    }

    public virtual void MissHit()
    {
        Judgement = RhythmGame.Judgement.Miss;
        HitTimeMs = null;
    }
}

public readonly struct HitWindow
{
    // Earliest hit time for the window
    [Range(float.MinValue, 0)] public readonly float LeftMs;

    // Latest hit time for the window
    [Range(0, float.MaxValue)] public readonly float RightMs;
    public readonly Judgement Judgement;

    public HitWindow(float leftMs, float rightMs, Judgement judgement)
    {
        LeftMs = leftMs;
        RightMs = rightMs;
        Judgement = judgement;
    }
}

public enum Judgement
{
    // None may represent cases where the judgement is missing or unknown. In general, null is preferred for this or for
    // notes that have not yet been judged, and None should not be used. However, None is included here as a sane value
    // for the default value of the enum.
    None = default,
    Miss,
    Good,
    Great,
    Marvelous,
}
}
