namespace SaturnGame.RhythmGame
{
[System.Serializable]
public class TouchNote : Note
{
    public TouchNote(
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

    public override HitWindow[] HitWindows => BaseHitWindows;
}
}
