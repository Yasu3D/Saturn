namespace SaturnGame.RhythmGame
{
[System.Serializable]
public class ChainNote : Note
{
    public ChainNote(
        int measure,
        int tick,
        int position,
        int size,
        int id,
        NoteBonusType bonusType = NoteBonusType.None,
        bool isSync = false
    ) : base(measure, tick, position, size, id, bonusType, isSync)
    {
    }

    private static HitWindow[] hitWindows =
    {
        new(-4 * FrameMs, 4 * FrameMs, RhythmGame.Judgement.Marvelous),
    };

    public override HitWindow[] HitWindows => hitWindows;

    // Note: for chain notes, the note cannot be "hit" before the time of the note, it can only be touched (see below).
    // As such, HitTimeMs may not be useful or reliable for most uses.

    // HasBeenTouched will be true if the note has been touched at any point during the hit window.
    // A note that has been touched will also be hit once we are past this.TimeMs.
    // This is needed to avoid hitting the note at the beginning of the hit window if the player is already holding
    // down some segment of the note.
    public bool HasBeenTouched;
}
}
