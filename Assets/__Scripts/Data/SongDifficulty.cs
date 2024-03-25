using System;
using JetBrains.Annotations;

public enum DifficultyName
{
    [UsedImplicitly] Normal,
    [UsedImplicitly] Hard,
    [UsedImplicitly] Expert,
    [UsedImplicitly] Inferno,
    [UsedImplicitly] Beyond,
}

[Serializable]
public struct SongDifficulty
{
    public bool Exists;
    [UsedImplicitly] public DifficultyName DiffName;
    public float DiffLevel;
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