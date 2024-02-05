using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [Serializable]
    public class HoldNote : Note
    {
        public HoldNote(HoldSegment start, HoldSegment[] segments, HoldSegment end)
        {
            Notes = new HoldSegment[] { start }.Concat(segments).Concat(new HoldSegment[] { end }).ToArray();
        }

        public HoldNote(HoldSegment[] segments)
        {
            Notes = segments;
        }

        /// <summary>
        /// Returns a "deep copy" of a Hold Note, where both the <br />
        /// Hold Note and all Child Notes are new objects.
        /// </summary>
        public static HoldNote DeepCopy(HoldNote hold)
        {
            HoldNote copy = (HoldNote) hold.Clone();

            List<HoldSegment> segments = new();

            foreach (HoldSegment note in hold.Notes)
                segments.Add((HoldSegment) note.Clone());

            copy.Notes = segments.ToArray();
            return copy;
        }

        public override void CalculateTime(List<Gimmick> bgmDataGimmicks)
        {
            base.CalculateTime(bgmDataGimmicks);

            foreach (HoldSegment note in Notes)
                note.CalculateTime(bgmDataGimmicks);
        }

        public override void CalculateScaledTime(List<Gimmick> hiSpeedGimmicks)
        {
            base.CalculateScaledTime(hiSpeedGimmicks);

            foreach (HoldSegment note in Notes)
                note.CalculateScaledTime(hiSpeedGimmicks);
        }

        public override void ReverseTime(float startTime, float midTime, float endTime)
        {
            base.ReverseTime(startTime, midTime, endTime);

            foreach (HoldSegment note in Notes)
                note.ReverseTime(startTime, midTime, endTime);
        }

        public override int Measure { get => Start.Measure; set => Start.Measure = value; }
        public override int Tick { get => Start.Tick; set => Start.Tick = value; }
        public override int Position { get => Start.Position; set => Start.Position = value; }
        public override int Size { get => Start.Size; set => Start.Size = value; }

        public HoldSegment Start => Notes[0];
        public HoldSegment End => Notes[^1];

        // Segments is all HoldSegments except Start and End;
        public HoldSegment[] Segments => Notes.Skip(1).Take(Notes.Length - 2).ToArray();
        // Notes is all HoldSegments, including Start and End;
        public HoldSegment[] Notes;
        //
        public HoldSegment[] RenderedNotes => Notes.Where(x => x.RenderFlag).ToArray();

        public int MaxSize => Notes.Max(note => note.Size);
    }
}
