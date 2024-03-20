using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SaturnGame.Loading
{
    public class MerLoader
    {
        /// <summary>
        /// Converts a Stream into a List of strings for parsing.
        /// </summary>
        public static List<string> LoadMer(Stream stream)
        {
            List<string> lines = new List<string>();
            StreamReader reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                lines.Add(reader.ReadLine() ?? "");
            return lines;
        }


        /// <summary>
        /// Finds Metadata tags like "#OFFSET" in a string and return whatever value comes after.
        /// </summary>
        public static string GetMetadata(string input, string tag)
        {
            if (input.Contains(tag))
                return input.Substring(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length).TrimStart();

            return null;
        }
    }

    public class AudioLoader
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
                using(UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, type))
                {
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        var handler = (DownloadHandlerAudioClip) webRequest.downloadHandler;
                        handler.streamAudio = true;
                        var test = handler.audioClip;
                        return test;
                    }
                    else
                    {
                        Debug.Log($"[BGM] Error loading: {webRequest.error}");
                    }
                }
            }
            catch(Exception error)
            {
                Debug.LogWarning($"[BGM] Audio load exception!\n{error.Message}, {error.StackTrace}");
            }

            Debug.Log("[BGM] Could not load file for unknown reason...");
            return null;
        }

        public static AudioType GetAudioType(string path)
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

    public class ImageLoader
    {
        public async static Task<Texture2D> LoadImageWebRequest(string path)
        {
            if (!File.Exists(path)) return null;

            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(Platform.PathToWebUri(path));
            await uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerTexture.GetContent(uwr);
            }
            else
            {
                Debug.Log($"[Image] Error loading: {uwr.error} for path {path}");
            }

            return null;
        }
    }
}
