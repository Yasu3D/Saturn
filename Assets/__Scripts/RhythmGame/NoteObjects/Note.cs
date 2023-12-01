using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
    [System.Serializable]
    public class Note : ChartObject
    {
        public Note (int measure, int tick, ObjectEnums.NoteType noteType, ObjectEnums.BonusType bonusType, int position, int size, bool renderFlag = true)
        {
            Measure = measure;
            Tick = tick;
            Position = position;
            Size = size;
            NoteType = noteType;
            BonusType = bonusType;
            RenderFlag = renderFlag;
        }

        public Note (int measure, int tick, int noteID, int position, int size, bool renderFlag = true)
        {
            Measure = measure;
            Tick = tick;
            Position = position;
            Size = size;
            RenderFlag = renderFlag;

            // assign noteType
            switch (noteID)
            {
                case 1:
                case 2:
                case 20:
                    NoteType = ObjectEnums.NoteType.Touch;
                    break;
                
                case 3:
                case 21:
                    NoteType = ObjectEnums.NoteType.SnapForward;
                    break;

                case 4:
                case 22:
                    NoteType = ObjectEnums.NoteType.SnapBackward;
                    break;
                
                case 5:
                case 6:
                case 23:
                    NoteType = ObjectEnums.NoteType.SwipeClockwise;
                    break;

                case 7:
                case 8:
                case 24:
                    NoteType = ObjectEnums.NoteType.SwipeCounterclockwise;
                    break;

                case 9:
                case 25:
                    NoteType = ObjectEnums.NoteType.HoldStart;
                    break;

                case 10:
                    NoteType = ObjectEnums.NoteType.HoldSegment;
                    break;

                case 11:
                    NoteType = ObjectEnums.NoteType.HoldEnd;
                    break;

                case 12:
                    NoteType = ObjectEnums.NoteType.MaskAdd;
                    break;

                case 13:
                    NoteType = ObjectEnums.NoteType.MaskRemove;
                    break;

                case 14:
                    NoteType = ObjectEnums.NoteType.EndChart;
                    break;

                case 16:
                case 26:
                    NoteType = ObjectEnums.NoteType.Chain;
                    break;

                default:
                    NoteType = ObjectEnums.NoteType.None;
                    break;
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
                    BonusType = ObjectEnums.BonusType.None;
                    break;
                
                case 2:
                case 6:
                case 8:
                    BonusType = ObjectEnums.BonusType.Bonus;
                    break;
                
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    BonusType = ObjectEnums.BonusType.R_Note;
                    break;

                default:
                    BonusType = ObjectEnums.BonusType.None;
                    break;
            }
        }
        [Range(0, 59)] public int Position;
        [Range(1, 60)] public int Size;
        public ObjectEnums.NoteType NoteType;
        public ObjectEnums.BonusType BonusType;
        public bool RenderFlag;
    }

}

