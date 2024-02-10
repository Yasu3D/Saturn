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
            // TODO: fix
            new HitWindow(-45f, 45f, RhythmGame.Judgement.Marvelous),
        };
        public override HitWindow[] HitWindows => _hitWindows;
    }
}
