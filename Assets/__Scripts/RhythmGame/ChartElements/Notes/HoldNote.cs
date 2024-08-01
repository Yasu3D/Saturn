using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace SaturnGame.RhythmGame
{
[Serializable]
public class HoldNote : Note
{
    // if using this constructor, you must add the remaining segments later.
    public HoldNote([NotNull] HoldSegment start, int id)
    {
        ID = id;
        Notes = new[] { start };
    }

    /// <summary>
    /// Returns a "deep copy" of a Hold Note, where both the <br />
    /// Hold Note and all Child Notes are new objects.
    /// </summary>
    [NotNull]
    public static HoldNote DeepCopy([NotNull] HoldNote hold)
    {
        HoldNote copy = (HoldNote)hold.Clone();

        copy.Notes = hold.Notes.Select(note => (HoldSegment)note.Clone()).ToArray();
        return copy;
    }

    public override void CalculateTime(List<Gimmick> bgmDataGimmicks)
    {
        base.CalculateTime(bgmDataGimmicks);

        foreach (HoldSegment note in Notes)
            note.CalculateTime(bgmDataGimmicks);
    }

    public override void CalculateScaledTime(List<Gimmick> hiSpeedGimmicks)
    {
        base.CalculateScaledTime(hiSpeedGimmicks);

        foreach (HoldSegment note in Notes)
            note.CalculateScaledTime(hiSpeedGimmicks);
    }

    public override void ReverseTime(float startTime, float midTime, float endTime)
    {
        base.ReverseTime(startTime, midTime, endTime);

        foreach (HoldSegment note in Notes)
            note.ReverseTime(startTime, midTime, endTime);
    }

    public override int Measure
    {
        get => Start.Measure;
        set => Start.Measure = value;
    }

    public override int Tick
    {
        get => Start.Tick;
        set => Start.Tick = value;
    }

    public override int Position
    {
        get => Start.Position;
        set => Start.Position = value;
    }

    public override int Size
    {
        get => Start.Size;
        set => Start.Size = value;
    }

    [NotNull] public HoldSegment Start => Notes[0];
    [NotNull] public HoldSegment End => Notes[^1];

    // Notes is all HoldSegments, including Start and End;
    [NotNull] public HoldSegment[] Notes;
    [NotNull] public HoldSegment[] RenderedNotes => Notes.Where(x => x.RenderFlag).ToArray();

    [NotNull]
    public HoldSegment CurrentSegmentFor(float currentTimeMs)
    {
        return Notes.Last(segment => segment.TimeMs <= currentTimeMs);
    }

    public int MaxSize => Notes.Max(note => note.Size);

    public override HitWindow[] HitWindows => BaseHitWindows;

    public override Judgement Hit(float hitTimeMs)
    {
        // Don't call super.Hit as it will set Judgement.
        HitWindow? activeHitWindow = ActiveHitWindow(hitTimeMs);
        if (activeHitWindow is not { Judgement: var judgement })
        {
            throw new ArgumentException($"Hit time {hitTimeMs} is not within any hit window for note at {TimeMs}",
                nameof(hitTimeMs));
        }

        StartJudgement = judgement;
        HitTimeMs = hitTimeMs;
        Held = true;
        Dropped = false;
        CurrentlyHeld = true;
        // If the hold is hit early, begin leniency window at the start of the hold.
        // If the hold is hit late, begin leniency window immediately.
        LastHeldTimeMs = Math.Max(hitTimeMs, TimeMs);
        return judgement;
    }

    public override void MissHit()
    {
        // Don't call super.MissHit as it will set Judgement.
        StartJudgement = RhythmGame.Judgement.Miss;
        HitTimeMs = null;
        Held = false;
        Dropped = false;
        CurrentlyHeld = false;
        // In this case, lastHeldTimeMs can be set to the beginning of the note, since
        // that's when the hold leniency window should begin.
        LastHeldTimeMs = TimeMs;
    }

    public Judgement? StartJudgement;
    public override bool HitWindowsEvaluated => StartJudgement is not null;
    public override bool HasBeenHit => StartJudgement is not null && StartJudgement != RhythmGame.Judgement.Miss;
    public bool CurrentlyHeld;

    /// <summary>
    /// LastHeldTimeMs describes the timestamp when a Hold Note was last held.
    /// This value should always be somewhere between the TimeMs of the Hold Start and Hold End.
    /// </summary>
    /// <remarks>
    /// - It may be null if the note has not been hit yet.<br/><br/>
    /// - If the hold start is missed (e.g. StartJudgement == Miss), it should be set to the TimeMs of the Hold Start until the note is touched.<br/><br/>
    /// - Hold Leniency is judged with this value by checking how much time has elapsed since the last time it was held.<br/><br/>
    /// </remarks>
    public float? LastHeldTimeMs;

    // Held should be true if the note was ever touched/held.
    public bool Held;

    // Dropped should be true if hold leniency is exceeded at any point in the hold.
    public bool Dropped;
    public const float LeniencyMs = 215f;

    // TODO: No clue if this is actually accurate.
    public Judgement Judge()
    {
        Debug.Assert(StartJudgement != null, $"Trying to an un-hit hold ({nameof(StartJudgement)} is null)");

        if (!Held)
            Judgement = RhythmGame.Judgement.Miss;
        else if (StartJudgement is RhythmGame.Judgement.Miss || Dropped)
            Judgement = RhythmGame.Judgement.Good;
        else
            Judgement = StartJudgement.Value;

        return Judgement.Value;
    }
}
}
