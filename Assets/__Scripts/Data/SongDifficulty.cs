using System;
using JetBrains.Annotations;

public enum Difficulty
{
    Normal = 0,
    Hard = 1,
    Expert = 2,
    Inferno = 3,
    Beyond = 4,
}

[Serializable]
public struct SongDifficulty
{
    public Difficulty Difficulty;
    public decimal Level;
    public string AudioFilepath;
    [UsedImplicitly] public float AudioOffset;

    // ReSharper disable NotAccessedField.Global - will be used later
    public string MovieFilepath;
    public float MovieOffset;
    // ReSharper restore NotAccessedField.Global

    public string ChartFilepath;
    public string Charter;
    public float PreviewStart;
    public float PreviewDuration;
}
