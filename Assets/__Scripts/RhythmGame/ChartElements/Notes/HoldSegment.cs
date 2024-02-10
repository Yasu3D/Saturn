namespace SaturnGame.RhythmGame
{
    /// <summary>
    /// HoldSegment represents a part of a hold, including the start and end.
    /// </summary>
    /// TODO: Split out start and end?
    /// Note: If you decide to make this a Note, update NoteColors.GetColorID to take a Note.
    [System.Serializable]
    public class HoldSegment : PositionedChartElement
    {
        public bool RenderFlag;

        public HoldSegment(int measure, int tick, int position, int size, bool renderFlag) : base(measure, tick, position, size)
        {
            RenderFlag = renderFlag;
        }
    }
}
