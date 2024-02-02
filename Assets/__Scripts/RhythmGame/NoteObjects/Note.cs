using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// Note represents any note type in the chart that needs to be hit and receives a judgement.
    /// </summary>
    /// Note (pun intended): For hold notes, the "position" is the the position of the start.
    [System.Serializable]
    public abstract class Note : PositionedChartObject
    {
        public ObjectEnums.BonusType BonusType;
        // IsSync is true if this note is a "sync" note, that is,
        // it is at the same time as another note and is highlighted
        public bool IsSync;

        public abstract ObjectEnums.NoteType NoteType { get; }

        public Note (
            int measure,
            int tick,
            int position,
            int size,
            ObjectEnums.BonusType bonusType,
            bool isSync = false) : base(measure, tick, position, size)
        {
            BonusType = bonusType;
            IsSync = isSync;
        }
    }
}
