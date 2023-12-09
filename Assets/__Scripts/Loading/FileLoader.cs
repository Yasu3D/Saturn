using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO.Enumeration;
using Microsoft.Unity.VisualStudio.Editor;

namespace SaturnGame.Loading
{
    public class MerLoader
    {
        /// <summary>
        /// Reads a <c>.mer</c> file at <c>path</c> and converts<br />
        /// it into a List of strings for parsing.
        /// </summary>
        public static List<string> LoadMer(string path)
        {
            if (!File.Exists(path)) return null;

            Stream stream = File.OpenRead(path);
            return LoadMer(stream);
        }


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
        /// Finds Metadata tags like "#OFFSET" in a string.
        /// </summary>
        public static string GetMetadata(string input, string tag)
        {
            if (input.Contains(tag))
                return input.Substring(input.IndexOf(tag, StringComparison.Ordinal) + tag.Length);

            return null;
        }
    }
    
    public class AudioLoader
    {
        public static async Task<AudioClip> LoadBgm(string path)
        {
            if (!File.Exists(path)) return null;

            AudioType type = await Task.Run(() => GetAudioType(path));
            
            if (type is AudioType.UNKNOWN) return null;

            Uri uri = new ("file://" + path);
            
            try
            {
                using(UnityWebRequest audioWebRequest = UnityWebRequestMultimedia.GetAudioClip(path, type))
                {
                    #pragma warning disable CS4014 // shut up the await warning
                    audioWebRequest.SendWebRequest();
                    #pragma warning restore CS4014

                    while(!audioWebRequest.isDone) await Task.Yield();

                    if (audioWebRequest.result == UnityWebRequest.Result.Success)
                        return DownloadHandlerAudioClip.GetContent(audioWebRequest);
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
        public static Texture2D LoadJacket(string path)
        {
            if (!File.Exists(path)) return null;

            Texture2D jacket = null;

            using (Stream stream = File.OpenRead(path))
            {
                MemoryStream memory = new();
                stream.CopyTo(memory);
                byte[] imageData = memory.ToArray();

                ImageConversion.LoadImage(jacket, memory.ToArray(), false);
            }

            return jacket;
        }
    }
}
