using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

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
            if (!File.Exists(path)) return null;

            AudioType type = GetAudioType(path);
            if (type is AudioType.UNKNOWN) return null;

            Uri uri = new ("file://" + path);
            try
            {
                using(UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, type))
                {
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        var test = DownloadHandlerAudioClip.GetContent(webRequest);
                        return test;
                    }
                }
            }
            catch(Exception error)
            {
                Debug.LogWarning($"Audio load error! {error.Message}, {error.StackTrace}");
            }

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
        /// <summary>
        /// Similar to <c>LoadJacket<c/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async static Task<Texture2D> LoadImageWebRequest(string path)
        {
            if (!File.Exists(path)) return null;
            
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
            await uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerTexture.GetContent(uwr);
            }

            return null;
        }
    }
}
