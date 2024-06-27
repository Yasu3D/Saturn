using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
/// <summary>
/// TimedTouchStateQueue implements a circular buffer queue of TimedTouchStates, managing allocation of the TouchStates.
/// TimedTouchStateQueue is not threadsafe.
/// </summary>
public class TimedTouchStateQueue
{
    // Capacity shouldn't need expansion. In prod, allow expansion to avoid crashing. In editor, throw instead.
    private readonly bool allowExpansion = !Application.isEditor;

    private TimedTouchState[] buffer;
    // Index of first element in the queue.
    private int head;
    // Index of next available slot.
    private int tail;
    // isEmpty is needed since head == tail can mean both empty and full.
    // isEmpty must be updated anytime the queue is modified.
    private bool isEmpty = true;

    public TimedTouchStateQueue(int capacity)
    {
        buffer = new TimedTouchState[capacity];

        // Allocate TouchStates
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = new()
            {
                TouchState = TouchState.CreateNew(),
            };
        }
    }

    public void Enqueue(TouchState touchState, float timeMs)
    {
        if (head == tail && !isEmpty)
        {
            if (!allowExpansion)
            {
                Debug.LogError($"TimedTouchStateQueue exceeded capacity {buffer.Length}.");
                throw new InvalidOperationException("Buffer is full");
                // terminal
            }

            Debug.LogWarning($"TimedTouchStateQueue exceeded capacity {buffer.Length}, expanding.");

            // Expand the buffer
            TimedTouchState[] newBuffer = new TimedTouchState[buffer.Length * 2];

            // Copy current queue into new buffer with the head at index 0
            for (int i = 0; i < buffer.Length; i++)
            {
                int oldBufferIndex = (head + i) % buffer.Length;
                newBuffer[i] = buffer[oldBufferIndex];
            }

            head = 0;
            tail = buffer.Length;

            // Allocate new TouchStates
            for (int i = buffer.Length; i < newBuffer.Length; i++)
            {
                newBuffer[i] = new()
                {
                    TouchState = TouchState.CreateNew(),
                };
            }

            buffer = newBuffer;
        }

        touchState.CopyTo(ref buffer[tail].TouchState);
        buffer[tail] = new(buffer[tail].TouchState, timeMs);
        tail = (tail + 1) % buffer.Length;

        isEmpty = false;
    }

    public IEnumerable<TimedTouchState> GetTimedTouchStatesUntil(float timeMs)
    {
        while (!isEmpty && buffer[head].TimeMs <= timeMs) yield return Dequeue()!.Value;
    }

    private TimedTouchState? Dequeue()
    {
        if (isEmpty) return null;

        TimedTouchState result = buffer[head];
        head = (head + 1) % buffer.Length;

        // If head=tail, it must be because we just removed the last element. Otherwise, the queue is not empty.
        isEmpty = head == tail;

        return result;
    }
}
}
