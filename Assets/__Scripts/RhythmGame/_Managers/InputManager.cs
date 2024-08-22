using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SaturnGame.Settings;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// InputManager handles **gameplay** inputs - reading from an <see cref="IInputProvider"/> such as <see
/// cref="TouchRingManager"/> or <see cref="keyboardInput"/>, or from a replay (via <see cref="ReplayManager"/>).
/// It pulls current input from these providers, associates it with the current gameplay time, buffers the input if
/// artificial latency is active, and then processes the input by calling into <see cref="scoringManager"/>.
/// </summary>
public class InputManager : Singleton<InputManager>, IInputProvider
{
    public enum InputSource
    {
        TouchRing,
        Keyboard,
        Replay,
    }

    public InputSource CurrentInputSource;

    [CanBeNull]
    private IInputProvider CurrentInputProvider => CurrentInputSource switch
    {
        InputSource.Keyboard => keyboardInput,
        InputSource.TouchRing => TouchRingManager.Instance,
        InputSource.Replay => null, // ReplayManager is not an IInputProvider.
        _ => throw new System.NotImplementedException(),
    };

    private static float LatencyMs => SettingsManager.Instance.PlayerSettings.GameSettings.CalculatedInputLatencyMs;

    private TimedTouchStateQueue queue;

    [Header("MANAGERS")] [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ReplayManager replayManager;

    private readonly KeyboardInput keyboardInput = new();

    public TouchState CurrentTouchState = TouchState.CreateNew();
    public TouchState GetCurrentTouchState() => CurrentTouchState;

    private void Start()
    {
        int queueSize = (int)LatencyMs switch
        {
            0 => 1,
            var n => n, // Queue size should be sufficient up to 1000fps (1ms per frame).
        };
        queue = new(queueSize);
    }


    // Warning: the provided TouchState's underlying data is not guaranteed to be valid past the end of this function's
    // invocation. A persisted reference to the TouchState may not behave as expected. Use TouchState.Copy or .CopyTo.
    // See docs on TouchState.
    private void HandleNewTouchState(TouchState? touchState, float timeMs)
    {
        if (touchState is null || touchState.Value.EqualsSegments(CurrentTouchState))
        {
            scoringManager.HandleInput(null, timeMs);
            // Don't write to replay.
            return;
        }

        if (replayManager != null && !replayManager.PlayingFromReplay)
            replayManager.RecordFrame(touchState.Value, timeMs);
        touchState.Value.CopyTo(ref CurrentTouchState);
        scoringManager.HandleInput(touchState.Value, timeMs);
    }


    /// <summary>
    /// Get touch states up to a given timeMs, INCLUSIVE.
    /// (n.b. float equality is usually not exact, but it's possible that the TimeMs in the queue exactly matches the
    /// current timeMs seen by the InputManager, e.g. if they are both happening on the same frame.)
    /// It can be assumed that the consumer will iterate through the IEnumerable to completion.
    /// </summary>
    /// <param name="timeMs"></param>
    /// <returns></returns>
    private IEnumerable<TimedTouchState> GetTimedTouchStatesUntil(float timeMs)
    {
        return CurrentInputSource switch
        {
            InputSource.Replay => replayManager.GetTimedTouchStatesUntil(timeMs),
            InputSource.TouchRing or InputSource.Keyboard => queue.GetTimedTouchStatesUntil(timeMs),
            _ => throw new System.NotImplementedException(),
        };
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Backslash))
        {
            Debug.Log("Switched to keyboard input.");
            CurrentInputSource = InputSource.Keyboard;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F11) && replayManager != null)
        {
            await replayManager.ReadReplayFile();
            Debug.Log("Loaded replay file, switching to replay input.");
            CurrentInputSource = InputSource.Replay;
            return;
        }

        if (timeManager.State != TimeManager.SongState.Playing) return;

        // Get the current input and queue it.
        switch (CurrentInputProvider?.GetCurrentTouchState())
        {
            case TouchState touchState:
            {
                queue.Enqueue(touchState, timeManager.GameplayTimeMs + LatencyMs);
                break;
            }
            case null:
            {
                // Current input method doesn't use an IInputProvider, e.g. Replay.
                break;
            }
        }

        // Actually handle inputs.
        foreach (TimedTouchState timedTouchState in GetTimedTouchStatesUntil(timeManager.GameplayTimeMs))
            HandleNewTouchState(timedTouchState.TouchState, timedTouchState.TimeMs);
    }
}

public struct TimedTouchState
{
    public TimedTouchState(TouchState touchState, float timeMs)
    {
        TouchState = touchState;
        TimeMs = timeMs;
    }

    public TouchState TouchState;
    public readonly float TimeMs;
}

// Abstract class for frame-synchronized input sources. (Not replays.) If/when we move to a multi-threaded model, this
// will need to be reworked.
public interface IInputProvider
{
    // Get the TouchState as of this frame. The TouchState is only guaranteed to live until the end of the frame, or
    // possibly until the next time GetCurrentTouchState() is called.
    public TouchState GetCurrentTouchState();
}

public class KeyboardInput : IInputProvider
{
    private TouchState currentTouchState = TouchState.CreateNew();

    private static void ReadFromKeyboard(bool[,] segments)
    {
        for (int i = 0; i < 60; i++)
        for (int j = 0; j < 4; j++)
            segments[i, j] = false;

        if (Input.GetKey("[6]"))
        {
            for (int i = 56; i < 60; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;

            for (int i = 0; i < 4; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[9]"))
        {
            for (int i = 4; i < 11; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[8]"))
        {
            for (int i = 11; i < 19; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[7]"))
        {
            for (int i = 19; i < 26; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[4]"))
        {
            for (int i = 26; i < 34; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[1]"))
        {
            for (int i = 34; i < 41; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[2]"))
        {
            for (int i = 41; i < 49; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        if (Input.GetKey("[3]"))
        {
            for (int i = 49; i < 56; i++)
            for (int j = 1; j < 3; j++)
                segments[i, j] = true;
        }

        // Thank you, ChatGPT
        // Top row (1-6)
        if (Input.GetKey("1")) segments[42, 3] = true;
        if (Input.GetKey("2")) segments[43, 3] = true;
        if (Input.GetKey("3")) segments[44, 3] = true;
        if (Input.GetKey("4")) segments[45, 3] = true;
        if (Input.GetKey("5")) segments[46, 3] = true;
        if (Input.GetKey("6")) segments[47, 3] = true;

        // Second row (q to y, mapping directly below 1-6)
        if (Input.GetKey("q")) segments[42, 2] = true;
        if (Input.GetKey("w")) segments[43, 2] = true;
        if (Input.GetKey("e")) segments[44, 2] = true;
        if (Input.GetKey("r")) segments[45, 2] = true;
        if (Input.GetKey("t")) segments[46, 2] = true;
        if (Input.GetKey("y")) segments[47, 2] = true;

        // Third row (a to h, mapping directly below q to y)
        if (Input.GetKey("a")) segments[42, 1] = true;
        if (Input.GetKey("s")) segments[43, 1] = true;
        if (Input.GetKey("d")) segments[44, 1] = true;
        if (Input.GetKey("f")) segments[45, 1] = true;
        if (Input.GetKey("g")) segments[46, 1] = true;
        if (Input.GetKey("h")) segments[47, 1] = true;

        // Bottom row (z to n, mapping directly below a to h)
        if (Input.GetKey("z")) segments[42, 0] = true;
        if (Input.GetKey("x")) segments[43, 0] = true;
        if (Input.GetKey("c")) segments[44, 0] = true;
        if (Input.GetKey("v")) segments[45, 0] = true;
        if (Input.GetKey("b")) segments[46, 0] = true;
        if (Input.GetKey("n")) segments[47, 0] = true;
    }

    public TouchState GetCurrentTouchState()
    {
        TouchState.StealAndUpdateSegments(ref currentTouchState, ReadFromKeyboard);
        return currentTouchState;
    }
}
}
