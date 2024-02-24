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
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
        }

        private static HitWindow[] _hitWindows = {
            new HitWindow(-4 * _FRAMEMS, 4 * _FRAMEMS, RhythmGame.Judgement.Marvelous),
        };
        public override HitWindow[] HitWindows => _hitWindows;
    }
}
