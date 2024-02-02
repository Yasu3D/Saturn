using System;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// Note represents any note type in the chart that needs to be hit and receives a judgement.
    /// </summary>
    /// Note (pun intended): For hold notes, the "position" is the the position of the start.
    [System.Serializable]
    public abstract class Note : PositionedChartElement
    {
        public NoteBonusType BonusType;
        // IsSync is true if this note is a "sync" note, that is,
        // it is at the same time as another note and is highlighted
        public bool IsSync;

        public Note (
            int measure,
            int tick,
            int position,
            int size,
            NoteBonusType bonusType,
            bool isSync = false) : base(measure, tick, position, size)
        {
            BonusType = bonusType;
            IsSync = isSync;
        }

        public static Note CreateFromNoteID(int measure, int tick, int noteID, int position, int size, bool renderFlag = true, bool isSync = false)
        {
            // TODO: remove null assignment
            Note note = null;

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
                    note.BonusType = NoteBonusType.None;
                    break;

                case 2:
                case 6:
                case 8:
                    note.BonusType = NoteBonusType.Bonus;
                    break;

                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    note.BonusType = NoteBonusType.R_Note;
                    break;

                default:
                    note.BonusType = NoteBonusType.None;
                    break;
            }

            return note;
        }

        public enum NoteBonusType
        {
            None,
            Bonus,
            R_Note,
        }
    }
}
