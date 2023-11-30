using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaturnGame.RhythmGame;
using UnityEngine;

[Serializable]
public class HoldNote
{
    public HoldNote(Note start, Note[] segments, Note end)
    {
        Start = start;
        Segments = segments;
        End = end;

        Notes = new Note[] { start }.Concat(segments).Concat(new Note[] { end }).ToArray();

        foreach (Note note in Segments)
            {
                if (note.Size > MaxSize)
                    MaxSize = note.Size;
            }

        TotalLength = Segments.Length + 2;
    }

    public HoldNote(Note[] segments)
    {
        Start = segments[0];
        End = segments[^1];
        Notes = segments;

        if (segments.Length == 2)
            Segments = new Note[0];

        if (segments.Length == 3)
            Segments = new Note[] { segments[1] };

        else if (segments.Length > 3)  
        {
            Segments = segments.Skip(1).Take(segments.Length - 2).ToArray();
            foreach (Note note in Segments)
            {
                if (note.Size > MaxSize)
                    MaxSize = note.Size;
            }
        }

        TotalLength = Segments.Length + 2;
    }

    public Note Start;
    public Note End;

    public Note[] Segments;
    public Note[] Notes;

    public int TotalLength;
    public int MaxSize;
}
