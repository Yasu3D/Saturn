using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace SaturnGame.Loading
{
public static class MerLoader
{
    /// <summary>
    /// Converts a Stream into a List of strings for parsing.
    /// </summary>
    [NotNull]
    public static List<string> LoadMer([NotNull] Stream stream)
    {
        List<string> lines = new();
        StreamReader reader = new(stream);
        while (!reader.EndOfStream)
            lines.Add(reader.ReadLine() ?? "");
        return lines;
    }


    /// <summary>
    /// Finds Metadata tags like "#OFFSET" in a string and return whatever value comes after.
    /// </summary>
    [CanBeNull]
    public static string GetMetadata([NotNull] string input, [NotNull] string tag)
    {
        return input.Contains(tag)
            ? input[(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length)..].TrimStart()
            : null;
    }
}

public static class AudioLoader
{
    public static async Task<AudioClip> LoadBgm(string path)
    {
        if (!File.Exists(path))
        {
            Debug.Log("[BGM] Couldn't load; file not found!");
            return null;
        }

        AudioType type = GetAudioType(path);
        if (type is AudioType.UNKNOWN)
        {
            Debug.Log("[BGM] Couldn't load; unknown file type!");
            return null;
        }

        Uri uri = Platform.PathToWebUri(path);
        try
        {
            using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, type);
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)webRequest.downloadHandler;
                handler.streamAudio = true;
                AudioClip test = handler.audioClip;
                return test;
            }

            Debug.Log($"[BGM] Error loading: {webRequest.error}");
        }
        catch (Exception error)
        {
            Debug.LogWarning($"[BGM] Audio load exception!\n{error.Message}, {error.StackTrace}");
        }

        Debug.Log("[BGM] Could not load file for unknown reason...");
        return null;
    }

    private static AudioType GetAudioType([CanBeNull] string path)
    {
        // Somewhat primitive approach, but works well enough.
        if (string.IsNullOrEmpty(path)) return AudioType.UNKNOWN;

        if (path.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase))
            return AudioType.OGGVORBIS;

        if (path.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase))
            return AudioType.WAV;

        if (path.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
            return AudioType.MPEG;

        return AudioType.UNKNOWN;
    }
}

public static class ImageLoader
{
    public static async Task<Texture2D> LoadImageWebRequest(string path)
    {
        if (!File.Exists(path)) return null;

        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(Platform.PathToWebUri(path));
        await uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
            return DownloadHandlerTexture.GetContent(uwr);

        Debug.Log($"[Image] Error loading: {uwr.error} for path {path}");
        return null;
    }
}
}