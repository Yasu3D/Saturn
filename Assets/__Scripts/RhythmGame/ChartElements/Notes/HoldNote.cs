using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [Serializable]
    public class HoldNote : Note
    {
        public HoldNote(HoldSegment start, HoldSegment[] segments, HoldSegment end) : base(start.Measure, start.Tick, start.Position, start.Size, NoteBonusType.None)
        {
            Start = start;
            Segments = segments;
            End = end;

            Notes = new HoldSegment[] { start }.Concat(segments).Concat(new HoldSegment[] { end }).ToArray();
            RenderedNotes = Notes.Where(x => x.RenderFlag).ToArray();

            foreach (HoldSegment note in Notes)
            {
                if (note.Size > MaxSize)
                    MaxSize = note.Size;
            }
        }

        public HoldNote(HoldSegment[] segments) : base(segments[0].Measure, segments[0].Tick, segments[0].Position, segments[0].Size, NoteBonusType.None)
        {
            Start = segments[0];
            End = segments[^1];
            Notes = segments;
            RenderedNotes = Notes.Where(x => x.RenderFlag).ToArray();

            if (segments.Length == 2)
                Segments = new HoldSegment[0];

            if (segments.Length == 3)
                Segments = new HoldSegment[] { segments[1] };

            else if (segments.Length > 3)
            {
                Segments = segments.Skip(1).Take(segments.Length - 2).ToArray();
            }

            foreach (HoldSegment note in Notes)
            {
                if (note.Size > MaxSize)
                    MaxSize = note.Size;
            }
        }

        /// <summary>
        /// Returns a "deep copy" of a Hold Note, where both the <br />
        /// Hold Note and all Child Notes are new objects.
        /// </summary>
        public static HoldNote DeepCopy(HoldNote hold)
        {
            List<HoldSegment> segments = new();

            foreach (HoldSegment note in hold.Notes)
                segments.Add((HoldSegment) note.Clone());

            return new(segments.ToArray());
        }

        public HoldSegment Start;
        public HoldSegment End;

        public HoldSegment[] Segments;
        public HoldSegment[] Notes;
        public HoldSegment[] RenderedNotes;

        public int MaxSize;
    }
}
