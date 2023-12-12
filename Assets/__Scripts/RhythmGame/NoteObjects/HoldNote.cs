using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [Serializable]
    public class HoldNote
    {
        public HoldNote(Note start, Note[] segments, Note end)
        {
            Start = start;
            Segments = segments;
            End = end;

            Notes = new Note[] { start }.Concat(segments).Concat(new Note[] { end }).ToArray();
            RenderedNotes = Notes.Where(x => x.RenderFlag).ToArray();

            foreach (Note note in Notes)
            {
                if (note.Size > MaxSize)
                    MaxSize = note.Size;
            }
        }

        public HoldNote(Note[] segments)
        {
            Start = segments[0];
            End = segments[^1];
            Notes = segments;
            RenderedNotes = Notes.Where(x => x.RenderFlag).ToArray();

            if (segments.Length == 2)
                Segments = new Note[0];

            if (segments.Length == 3)
                Segments = new Note[] { segments[1] };

            else if (segments.Length > 3)  
            {
                Segments = segments.Skip(1).Take(segments.Length - 2).ToArray();
            }

            foreach (Note note in Notes)
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
            List<Note> segments = new();

            foreach (Note note in hold.Notes)
                segments.Add(new(note));

            return new(segments.ToArray());
        }

        public Note Start;
        public Note End;

        public Note[] Segments;
        public Note[] Notes;
        public Note[] RenderedNotes;

        public int MaxSize;
    }
}
