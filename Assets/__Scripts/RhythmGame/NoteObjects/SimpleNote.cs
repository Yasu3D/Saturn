using System;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// SimpleNote is a base class for notes that have a fixed size, position, and time. (Basically any notes except hold notes.)
    /// </summary>
    [System.Serializable]
    public abstract class SimpleNote : Note
    {
        public SimpleNote(SimpleNote note) : base(note.Measure, note.Tick, note.Position, note.Size, note.BonusType, note.IsSync)
        {
            TimeMs = note.TimeMs;
            ScaledVisualTime = note.ScaledVisualTime;
        }

        public SimpleNote(int measure, int tick, ObjectEnums.BonusType bonusType, int position, int size, bool renderFlag = true, bool isSync = false) : base(measure, tick, position, size, bonusType, isSync)
        {
        }

        public static SimpleNote CreateFromNoteID(int measure, int tick, int noteID, int position, int size, bool renderFlag = true, bool isSync = false)
        {
            // TODO: remove null assignment
            SimpleNote note = null;

            // TODO: fix this stuff
            switch (noteID)
            {
                case 1:
                case 2:
                case 20:
                    note = new TouchNote(measure, tick, position, size);
                    break;

                case 3:
                case 21:
                    note = new SnapNote(measure, tick, position, size, SnapNote.SnapDirection.Forward);
                    break;

                case 4:
                case 22:
                    note = new SnapNote(measure, tick, position, size, SnapNote.SnapDirection.Backward);
                    break;

                case 5:
                case 6:
                case 23:
                    note = new SwipeNote(measure, tick, position, size, SwipeNote.SwipeDirection.Clockwise);
                    break;

                case 7:
                case 8:
                case 24:
                    note = new SwipeNote(measure, tick, position, size, SwipeNote.SwipeDirection.Counterclockwise);
                    break;

                // case 9:
                // case 25:
                //     NoteType = ObjectEnums.NoteType.HoldStart;
                //     break;

                // case 10:
                //     NoteType = ObjectEnums.NoteType.HoldSegment;
                //     break;

                // case 11:
                //     NoteType = ObjectEnums.NoteType.HoldEnd;
                //     break;

                // case 12:
                //     NoteType = ObjectEnums.NoteType.MaskAdd;
                //     break;

                // case 13:
                //     NoteType = ObjectEnums.NoteType.MaskRemove;
                //     break;

                // case 14:
                //     NoteType = ObjectEnums.NoteType.EndChart;
                //     break;

                case 16:
                case 26:
                    note = new ChainNote(measure, tick, position, size);
                    break;

                default:
                    // NoteType = ObjectEnums.NoteType.None;
                    break;
            }

            if (note is null)
            {
                throw new Exception($"wrong simplenote typeid {noteID}");
            }

            // assign bonusType
            switch (noteID)
            {
                case 1:
                case 3:
                case 4:
                case 5:
                case 7:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 16:
                    note.BonusType = ObjectEnums.BonusType.None;
                    break;

                case 2:
                case 6:
                case 8:
                    note.BonusType = ObjectEnums.BonusType.Bonus;
                    break;

                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    note.BonusType = ObjectEnums.BonusType.R_Note;
                    break;

                default:
                    note.BonusType = ObjectEnums.BonusType.None;
                    break;
            }

            return note;
        }
    }
}
