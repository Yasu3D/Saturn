using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SaturnGame.RhythmGame
{
    [Serializable]
    public class Chart
    {
        [UsedImplicitly] public float Difficulty = 0;
        [UsedImplicitly] public float ClearThreshold = 0;
        public float AudioOffset = 0;
        [UsedImplicitly] public float MovieOffset = 0;

        public List<Gimmick> BGMDataGimmicks = new();
        public List<Gimmick> HiSpeedGimmicks = new();
        public List<Gimmick> ReverseGimmicks = new();
        public List<Note> Notes = new();
        public List<HoldNote> HoldNotes = new();
        public List<Mask> Masks = new();
        public List<BarLine> BarLines = new();
        public List<SyncIndicator> Syncs = new();
        public EndOfChart EndOfChart;
        public List<Note> ReverseNotes = new();
        public List<HoldNote> ReverseHoldNotes = new();

    }
}
