namespace SaturnGame.RhythmGame
{
    [System.Serializable] public class TimeSignature
    {
        public TimeSignature(int upper, int lower)
        {
            Upper = upper;
            Lower = lower;
            Ratio = (float) upper / (float) lower;
        }

        public int Upper = 4;
        public int Lower = 4;
        public float Ratio = 1;

        public static TimeSignature Default { get; private set; } = new(4,4);

        public override string ToString()
        {
            return $"{Upper}/{Lower} ({Ratio})";
        }
    }
}