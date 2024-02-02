using UnityEngine;

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
            NoteBonusType bonusType = NoteBonusType.None,
            bool isSync = false
            ) : base(measure, tick, position, size, bonusType, isSync)
        {
        }
    }
}
