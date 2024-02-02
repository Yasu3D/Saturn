using System;
using System.Collections.Generic;

namespace SaturnGame.RhythmGame
{
    [Serializable]
    public class Chart
    {
        public float difficulty = 0;
        public float clearThreshold = 0;
        public float audioOffset = 0;
        public float movieOffset = 0;

        public List<Gimmick> bgmDataGimmicks = new();
        public List<Gimmick> hiSpeedGimmicks = new();
        public List<Gimmick> reverseGimmicks = new();
        public List<SimpleNote> notes = new();
        public List<HoldNote> holdNotes = new();
        public List<Mask> masks = new();
        public List<BarLine> barLines = new();
        public List<SyncIndicator> syncs = new();
        public EndOfChart endOfChart;
        public List<SimpleNote> reverseNotes = new();
        public List<HoldNote> reverseHoldNotes = new();

    }
}
