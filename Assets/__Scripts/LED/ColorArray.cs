using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.LED
{
public class ColorArray
{
    public readonly List<ScaFrame> Frames;
    public readonly int Depth;
    public readonly int Width;

    private ColorArray(List<ScaFrame> frames, int depth, int width)
    {
        Frames = frames;
        Depth = depth;
        Width = width;
    }

    [NotNull]
    public static ColorArray Parse(string path)
    {
        return ParseWithSize(path, 8, 60);
    }

    [NotNull]
    public static ColorArray ParseWithSize(string path, int depth, int width)
    {
        List<ScaFrame> colors = new();
        List<string> lines = new();

        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

        using Stream stream = File.OpenRead(path);
        using StreamReader streamReader = new(stream);
        while (!streamReader.EndOfStream) lines.Add(streamReader.ReadLine() ?? "");

        ScaFrame frame = new() { Colors = new Color[depth, width] };

        int fileIndex = 0;
        int lineIndex = 0;

        while (fileIndex < lines.Count)
        {
            string[] parsed = lines[fileIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (parsed.Length is 1)
            {
                if (parsed[0] != "#")
                    throw new ArgumentOutOfRangeException($"Line {fileIndex} contains an invalid character.");

                colors.Add(frame);

                frame = new() { Colors = new Color[depth, width] };

                fileIndex++;
                lineIndex = 0;
                continue;
            }

            for (int i = 0; i < depth; i++) frame.Colors[i, lineIndex] = SaturnMath.HexToColor32($"#{parsed[i]}");

            fileIndex++;
            lineIndex++;
        }

        return new(colors, depth, width);
    }
}


[Serializable]
public class ScaFrame
{
    public Color[,] Colors = new Color[8, 60];
}
}
