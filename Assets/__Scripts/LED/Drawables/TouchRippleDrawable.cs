using SaturnGame.LED;
using UnityEngine;

namespace SaturnGame
{
    public class TouchRippleDrawable : LedDrawable
    {
        public int TouchPosition;
        public TouchRipplePool Pool;
        
        public bool Playing;
        private const int Framerate = 60;
        [SerializeField] private int frame;
        
        private async void Animate()
        {
            const float interval = 1.0f / Framerate;

            while (Playing)
            {
                frame++;
                if (frame >= Pool.Frames.Count) Stop();

                await Awaitable.WaitForSecondsAsync(interval);
            }
        }
        
        public override void Draw(ref Color32[,] data)
        {
            if (!Playing) return;
            
            const int offsetX = -4;
            const int offsetY = -6;
            
            int clampedFrame = Mathf.Clamp(frame, 0, Mathf.Max(Pool.Frames.Count - 1, 0));
            
            for (int i = 0; i < 14; i++)
            for (int j = 0; j < 9; j++)
            {
                int y = i + offsetY + TouchPosition * 2 % 8;
                int x = SaturnMath.Modulo(j + offsetX + TouchPosition / 4, 60);

                if (y is > 7 or < 0) continue;
                
                data[y, x] += Pool.Frames[clampedFrame].Colors[i, j];
            }
        }

        public void Play()
        {
            frame = 0;
            
            if (!Playing)
            {
                Playing = true;
                Animate();
            }
            
            Enabled = true;
        }
        
        public void Stop()
        {
            Playing = false;
            Enabled = false;

            Pool.ReleaseObject(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I)) Play();
        }
    }
}
