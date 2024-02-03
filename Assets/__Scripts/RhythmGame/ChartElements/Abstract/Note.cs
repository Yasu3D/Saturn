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

        protected Note()
        {
        }

        public void SetBonusTypeFromNoteID(int noteID)
        {
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
                    BonusType = NoteBonusType.None;
                    break;

                case 2:
                case 6:
                case 8:
                    BonusType = NoteBonusType.Bonus;
                    break;

                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    BonusType = NoteBonusType.R_Note;
                    break;

                default:
                    throw new ArgumentException($"Unkown note ID {noteID}", "noteID");
            }
        }

        public static Note CreateFromNoteID(int measure, int tick, int noteID, int position, int size, bool renderFlag = true, bool isSync = false)
        {
            Note note;

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


                case 16:
                case 26:
                    note = new ChainNote(measure, tick, position, size);
                    break;

                case 9:
                case 25:
                case 10:
                case 11:
                    throw new ArgumentException($"Note ID {noteID} represents a HoldNote component which is unsupported by this function", "noteID");

                case 12:
                case 13:
                case 14:
                    throw new ArgumentException($"Note ID {noteID} does not represent a Note", "noteID");

                default:
                    throw new ArgumentException($"Unkown note ID {noteID}", "noteID");
            }

            note.SetBonusTypeFromNoteID(noteID);

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
