using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class InputManager : MonoBehaviour
{
    public enum InputSource
    {
        TouchRing,
        Keyboard,
        Replay,
    }

    public InputSource CurrentInputSource;

    [Header("MANAGERS")]
    [SerializeField] private ScoringManager scoringManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private ReplayManager replayManager;

    private readonly KeyboardInput keyboardInput = new();

    public TouchState CurrentTouchState = TouchState.CreateNew();

    // Warning: the provided TouchState's underlying data is not guaranteed to be valid past the end of this function's
    // invocation. A persisted copy of TouchState may not behave as expected. See docs on TouchState.
    private void MaybeHandleNewTouchState(InputSource inputSource, TouchState touchState, float? timeMs)
    {
        if (CurrentInputSource == inputSource)
            NewTouchState(touchState, timeMs ?? timeManager.VisualTimeMs);
    }

    private void NewTouchState(TouchState touchState, float timeMs)
    {
        CurrentTouchState = touchState;
        if (replayManager != null && !replayManager.PlayingFromReplay)
            replayManager.RecordFrame(touchState, timeMs);
        scoringManager.HandleInput(touchState, timeMs);
        touchState.CopyTo(ref CurrentTouchState);
    }

    private void Start()
    {
        keyboardInput.TouchStateHandler = (touchState, timeMs) =>
            MaybeHandleNewTouchState(InputSource.Keyboard, touchState, timeMs);

        if (TouchRingManager.Instance is TouchRingManager touchRingManager)
        {
            touchRingManager.TouchStateHandler = (touchState, timeMs) =>
                MaybeHandleNewTouchState(InputSource.TouchRing, touchState, timeMs);
        }

        if (replayManager != null)
        {
            replayManager.TouchStateHandler = (touchState, timeMs) =>
                MaybeHandleNewTouchState(InputSource.Replay, touchState, timeMs);
        }
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
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

        if (CurrentInputSource is InputSource.Keyboard)
            keyboardInput.UpdateKeyboardInput();
    }
}

// Warning: the provided TouchState's underlying data is not guaranteed to be valid past the end of the handler invocation.
public delegate void TouchStateHandler(TouchState touchState, float? timeMs);

public interface IInputProvider
{
    // TouchStateHandler may be called any number of times per frame.
    [CanBeNull] public TouchStateHandler TouchStateHandler { set; }
}

public class KeyboardInput : IInputProvider
{
    public TouchStateHandler TouchStateHandler { private get; set; }
    private TouchState currentTouchState = TouchState.CreateNew();

    private static void ReadFromKeyboard(bool[,] segments)
    {
        foreach (int i in Enumerable.Range(0, 60))
        foreach (int j in Enumerable.Range(0, 4))
            segments[i, j] = false;

        if (Input.GetKey("[6]"))
        {
            foreach (int i in Enumerable.Range(56, 4))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;

            foreach (int i in Enumerable.Range(0, 4))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[9]"))
        {
            foreach (int i in Enumerable.Range(4, 7))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[8]"))
        {
            foreach (int i in Enumerable.Range(11, 8))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[7]"))
        {
            foreach (int i in Enumerable.Range(19, 7))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[4]"))
        {
            foreach (int i in Enumerable.Range(26, 8))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[1]"))
        {
            foreach (int i in Enumerable.Range(34, 7))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[2]"))
        {
            foreach (int i in Enumerable.Range(41, 8))
            foreach (int j in Enumerable.Range(1, 2))
                segments[i, j] = true;
        }

        if (Input.GetKey("[3]"))
        {
            foreach (int i in Enumerable.Range(49, 7))
            foreach (int j in Enumerable.Range(1, 2))
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

    public void UpdateKeyboardInput()
    {
        TouchState.StealAndUpdateSegments(ref currentTouchState, ReadFromKeyboard);
        TouchStateHandler?.Invoke(currentTouchState, null);
    }
}
}
