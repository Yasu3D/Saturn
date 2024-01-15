using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyName
{
    Normal, Hard, Expert, Inferno, Beyond
}

[Serializable]
public struct SongDifficulty
{
    public bool exists;
    public DifficultyName diffName;
    public float diffLevel;
    public string audioFilepath;
    public float audioOffset;
    public string movieFilepath;
    public float movieOffset;
    public string chartFilepath;
    public string charter;
    public float previewStart;
    public float previewDuration;
}
