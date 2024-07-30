using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;

namespace SaturnGame.Rendering
{
public class KeyBeamRenderer : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> laneSegments;
    [SerializeField] private InputManager inputManager;
    
    private static readonly int NoteWidthPropertyID = Shader.PropertyToID("_NoteWidth");
    private static readonly int ClipJudgementLinePropertyID = Shader.PropertyToID("_ClipJudgementLine");

    private void Awake()
    {
        foreach (MeshRenderer segment in laneSegments)
        {
            segment.material = new(segment.material);
            segment.material.SetFloat(NoteWidthPropertyID, SettingsManager.Instance.PlayerSettings.DesignSettings.NoteWidth);
        }
    }
    
    private void Update()
    {
        TouchState touchState = inputManager.CurrentTouchState;
        for (int anglePos = 0; anglePos < 60; anglePos++)
            laneSegments[anglePos].gameObject.SetActive(touchState.AnglePosPressedAtAnyDepth(anglePos));
    }

    public void SetClip(int id, bool state)
    {
        laneSegments[id].material.SetInteger(ClipJudgementLinePropertyID, state ? 1 : 0);
    }
}
}
