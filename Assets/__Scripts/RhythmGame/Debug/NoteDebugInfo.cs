using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SaturnGame.RhythmGame
{
public class NoteDebugInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int maxLines;
    [SerializeField] private List<string> infoQueue;
    [SerializeField] private bool needsUpdate;


    public void AddInfo(string info)
    {
        needsUpdate = true;
        // Remove lines at the end until we are below the max line count.
        // If things are working correctly, we shouldn't ever exceed maxLines anyway, so this will just remove one line.
        while (infoQueue.Count >= maxLines) infoQueue.RemoveAt(infoQueue.Count - 1);
        // Insert new info at the head of the queue.
        infoQueue.Insert(0, info);
    }

    private void Update()
    {
        // TODO: avoid allocation here. for now, avoid updating when it's not needed
        if (!needsUpdate) return;
        text.text = string.Join("\n", infoQueue);
        needsUpdate = false;
    }
}
}
