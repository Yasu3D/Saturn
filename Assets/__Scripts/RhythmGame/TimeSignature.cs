namespace SaturnGame.RhythmGame
{
[System.Serializable]
public class TimeSignature
{
    public TimeSignature(int upper, int lower)
    {
        Upper = upper;
        Lower = lower;
        Ratio = (float)upper / lower;
    }

    public int Upper;
    public int Lower;
    public float Ratio;

    public override string ToString()
    {
        return $"{Upper}/{Lower} ({Ratio})";
    }
}
}