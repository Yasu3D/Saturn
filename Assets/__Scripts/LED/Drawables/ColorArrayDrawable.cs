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

                int fileIndex = 0;
                int lineIndex = 0;

                while (fileIndex < lines.Count)
                {
                    string[] parsed = lines[fileIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    
                    if (parsed.Length is 1)
                    {
                        if (parsed[0] != "#") throw new ArgumentOutOfRangeException($"Line {fileIndex} contains an invalid character.");

                        colors.Add(frame);

                        frame = new();

                        fileIndex++;
                        lineIndex = 0;
                        continue;
                    }
                    
                    for (int i = 0; i < 8; i++)
                    {
                        frame.Colors[i, lineIndex] = SaturnMath.HexToColor32($"#{parsed[i]}");
                    }
                    
                    fileIndex++;
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
                if (Frame >= frames.Count)
                {
                    if (Loop) Frame = 0;
                    else Stop();
                }

                await Awaitable.WaitForSecondsAsync(interval);
            }
        }
        
        public override void Draw(ref Color32[,] data)
        {
            if (!Playing) return;
            
            int clampedFrame = Mathf.Clamp(Frame, 0, Mathf.Max(frames.Count - 1, 0));
            
            for (int i = 0; i < 8; i++)
            for (int j = 0; j < 60; j++)
            {
                data[i, j] = frames[clampedFrame].Colors[i, j];
            }
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Play();
        }
    }

    [Serializable]
    public class ScaFrame
    {
        public Color[,] Colors = new Color[8,60];
    }
}
