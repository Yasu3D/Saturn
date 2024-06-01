using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SaturnGame.LED;
using UnityEngine;

namespace SaturnGame
{
    public class TouchRipplePool : MonoBehaviourPool<TouchRippleDrawable>
    {
        [SerializeField] private string scaPath;
        public List<ScaFrame> Frames;
        
        private void Awake()
        {
            Frames = readFile(Path.Combine(Application.streamingAssetsPath, scaPath));
            return;

            static List<ScaFrame> readFile(string path)
            {
                List<ScaFrame> colors = new();
                List<string> lines = new();
            
                if (string.IsNullOrEmpty(path)) return colors;
            
                Stream stream = File.OpenRead(path);
                using StreamReader streamReader = new(stream);
                while (!streamReader.EndOfStream) lines.Add(streamReader.ReadLine() ?? "");

                ScaFrame frame = new() { Colors = new Color[14,9]};

                int fileIndex = 0;
                int lineIndex = 0;

                while (fileIndex < lines.Count)
                {
                    string[] parsed = lines[fileIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    
                    if (parsed.Length is 1)
                    {
                        if (parsed[0] != "#") throw new ArgumentOutOfRangeException($"Line {fileIndex} contains an invalid character.");

                        colors.Add(frame);

                        frame = new() { Colors = new Color[14,9]};

                        fileIndex++;
                        lineIndex = 0;
                        continue;
                    }
                    
                    for (int i = 0; i < 14; i++)
                    {
                        frame.Colors[i, lineIndex] = SaturnMath.HexToColor32($"#{parsed[i]}");
                    }
                    
                    fileIndex++;
                    lineIndex++;
                }

                return colors;
            }
        }
    }
}
