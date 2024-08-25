using JetBrains.Annotations;
using SaturnGame.LED;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame
{
/// <summary>
/// Lights up segments that are being touched.
/// </summary>
public class TouchStateDrawable : LedDrawable
{
    [CanBeNull]
    private static IInputProvider InputProvider => InputManager.Instance as IInputProvider ?? TouchRingManager.Instance;

    [SerializeField] private TouchRipplePool touchRipplePool;
    [SerializeField] private LedCompositor touchRippleCompositor;
    [SerializeField] private float blinkSpeed = 2; // radians/s, NOT Hz

    private TouchState touchState = TouchState.CreateNew();
    private TouchState prevTouchState = TouchState.CreateNew();

    [SerializeField] private Color colorA = new(1.0f, 1.0f, 1.0f, 1.0f);
    [SerializeField] private Color colorB = new(0.85f, 0.85f, 0.85f, 1.0f);
    private Color GetColor() => Color.Lerp(colorA, colorB, SaturnMath.PositiveSine(Time.time * blinkSpeed));

    private readonly Color[,] colorData = new Color[8, 60];

    private void Update()
    {
        if (InputProvider == null) return;

        touchState.CopyTo(ref prevTouchState);
        InputProvider.GetCurrentTouchState().CopyTo(ref touchState);

        for (int anglePos = 0; anglePos < 60; anglePos++)
        for (int depthPos = 0; depthPos < 4; depthPos++)
        {
            if (!touchState.IsPressed(anglePos, depthPos) || prevTouchState.IsPressed(anglePos, depthPos)) continue;

            TouchRippleDrawable drawable = touchRipplePool.GetObject();
            drawable.Pool = touchRipplePool;
            drawable.SetCompositor(touchRippleCompositor);
            drawable.Layer = Layer - 1;
            drawable.TouchAnglePos = anglePos;
            drawable.TouchDepthPos = depthPos;
            drawable.gameObject.SetActive(true);

            drawable.Play();
        }
    }

    public override Color[,] Draw()
    {
        LedCompositor.ClearCanvas(colorData, Color.clear);

        // Draw all ripples, with max opacity of 1.0. This acts as a cap, so that when the opacity is reduced later,
        // it stays below the given value.
        Color[,] ripples = touchRippleCompositor.Draw();

        for (int anglePos = 0; anglePos < 60; anglePos++)
        for (int ledNum = 0; ledNum < 8; ledNum++)
        {
            if (touchState.IsPressed(anglePos, ledNum / 2))
                // If this segment is pressed, draw the touch color
                colorData[ledNum, anglePos] = GetColor();
            else
            {
                // Use the ripples, but reduce the opacity.
                if (ripples == null) continue;
                colorData[ledNum, anglePos] = ripples[ledNum, anglePos];
                colorData[ledNum, anglePos].a *= 0.7f;
            }
        }

        return colorData;
    }
}
}
