using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class ChainNote : SimpleNote
    {
        public ChainNote(
            int measure,
            int tick,
            int position,
            int size,
            ObjectEnums.BonusType bonusType = ObjectEnums.BonusType.None,
            bool isSync = false
            ) : base(measure, tick, bonusType, position, size, isSync)
        {
        }

        public override ObjectEnums.NoteType NoteType => ObjectEnums.NoteType.Chain;
    }
}
