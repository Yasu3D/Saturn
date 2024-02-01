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
        public List<Note> notes = new();
        public List<HoldNote> holdNotes = new();
        public List<Note> masks = new();
        public List<ChartObject> barLines = new();
        public List<Note> syncs = new();
        public ChartObject endOfChart;
        public List<Note> reverseNotes = new();
        public List<HoldNote> reverseHoldNotes = new();
        

    }
}