using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaturnGame.LED
{
    /// <summary>
    /// Draws a .sca file. Can either be one frame at framerate 0, or an animation with a user-definable framerate and the option to loop.
    /// </summary>
    public class ColorArrayDrawable : LedDrawable
    {
        [SerializeField] private string scaPath;
        [SerializeField] private List<ScaFrame> frames;
        
        public bool Playing;
        public bool Loop;
        public int Framerate = 30;
        public int Frame;
        
        private void OnEnable()
        {
            frames = readFile(Path.Combine(Application.streamingAssetsPath, scaPath));
            return;

            static List<ScaFrame> readFile(string path)
            {
                List<ScaFrame> colors = new();
                List<string> lines = new();
            
                if (string.IsNullOrEmpty(path)) return colors;
            
                Stream stream = File.OpenRead(path);
                using StreamReader streamReader = new(stream);
                while (!streamReader.EndOfStream) lines.Add(streamReader.ReadLine() ?? "");

                ScaFrame frame = new();

                int lineIndex = 0;
                int colorIndex = 0;

                while (lineIndex < lines.Count)
                {
                    string[] parsed = lines[lineIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                    if (parsed.Length is 1)
                    {
                        if (parsed[0] != "#") throw new ArgumentOutOfRangeException($"Line {lineIndex} contains an invalid character.");

                        colors.Add(frame);

                        frame = new();
                        for (int i = 0; i < 480; i++) frame.Colors[i] = Color.black;

                        colorIndex = 0;
                        lineIndex++;
                        continue;
                    }

                    frame.Colors[colorIndex]     = SaturnMath.HexToColor32($"#{parsed[0]}");
                    frame.Colors[colorIndex + 1] = SaturnMath.HexToColor32($"#{parsed[1]}");
                    frame.Colors[colorIndex + 2] = SaturnMath.HexToColor32($"#{parsed[2]}");
                    frame.Colors[colorIndex + 3] = SaturnMath.HexToColor32($"#{parsed[3]}");
                    frame.Colors[colorIndex + 4] = SaturnMath.HexToColor32($"#{parsed[4]}");
                    frame.Colors[colorIndex + 5] = SaturnMath.HexToColor32($"#{parsed[5]}");
                    frame.Colors[colorIndex + 6] = SaturnMath.HexToColor32($"#{parsed[6]}");
                    frame.Colors[colorIndex + 7] = SaturnMath.HexToColor32($"#{parsed[7]}");
                
                    colorIndex += 8;
                    lineIndex++;
                }

                return colors;
            }
        }

        private void OnValidate()
        {
            Framerate = Mathf.Max(Framerate, 0);
        }
        
        private async void Animate()
        {
            if (Framerate == 0) return;
            float interval = 1.0f / Framerate;
            
            while (Playing)
            {
                Frame++;
                if (Frame == frames.Count)
                {
                    if (Loop) Frame = 0;
                    else Stop();
                }

                await Awaitable.WaitForSecondsAsync(interval);
            }
        }
        
        public override void Draw(ref Color32[] data)
        {
            int clampedFrame = Mathf.Clamp(Frame, 0, Mathf.Max(frames.Count - 1, 0));
            for (int i = 0; i < 480; i++) data[i] = frames[clampedFrame].Colors[i];
        }

        public void Play(bool restart = true)
        {
            if (restart) Frame = 0;
            
            if (!Playing)
            {
                Playing = true;
                Animate();
            }
            
            Enabled = true;
        }

        public void Pause()
        {
            Playing = false;
        }
        
        public void Stop()
        {
            Playing = false;
            Enabled = false;
        }
    }

    [Serializable]
    public class ScaFrame
    {
        public Color32[] Colors = new Color32[480];
    }
}
