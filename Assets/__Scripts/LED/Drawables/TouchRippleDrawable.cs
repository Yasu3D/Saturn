using System;
using System.Collections.Generic;
using System.IO;
using SaturnGame.LED;
using UnityEngine;

namespace SaturnGame
{
    public class TouchRippleDrawable : LedDrawable
    {
        public int TouchPosition;
        
        [SerializeField] private string scaPath;
        [SerializeField] private List<ScaFrame> frames;
        
        public bool Playing;
        private const int Framerate = 60;
        [SerializeField] private int frame;
        
        /*private void Awake()
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

                ScaFrame frame = new() { Colors = new Color32[126]};

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
                        for (int i = 0; i < 126; i++) frame.Colors[i] = Color.black;

                        colorIndex = 0;
                        lineIndex++;
                        continue;
                    }

                    for (int i = 0; i < 14; i++)
                    {
                        frame.Colors[colorIndex + i] = SaturnMath.HexToColor32($"#{parsed[i]}");
                    }

                    colorIndex += 14;
                    lineIndex++;
                }

                return colors;
            }
        }*/
        
        private async void Animate()
        {
            const float interval = 1.0f / Framerate;

            while (Playing)
            {
                frame++;
                if (frame == frames.Count) Stop();

                await Awaitable.WaitForSecondsAsync(interval);
            }
        }
        
        public override void Draw(ref Color32[,] data)
        {
            int clampedFrame = Mathf.Clamp(frame, 0, Mathf.Max(frames.Count - 1, 0));

            const int height = 14;
            const int width = 9;
            
            for (int x = 0; x < 9; x++)
            for (int y = 0; y < 8; y++)
            {
                int angle = SaturnMath.Modulo(x + 0, 60); // +pos
                int depth = Mathf.Min(y + 0, 7); // +depth
                
                //data[angle * 8 + depth] = frames[clampedFrame].Colors[];
            }
        }

        public void Play(bool restart = true)
        {
            if (restart) frame = 0;
            
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) Play();
        }
    }
}
