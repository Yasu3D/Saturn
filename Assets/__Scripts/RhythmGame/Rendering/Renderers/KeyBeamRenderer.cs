using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SaturnGame.RhythmGame;

namespace SaturnGame.Rendering
{
public class KeyBeamRenderer : MonoBehaviour
{
    [SerializeField] private List<GameObject> laneSegments;
    [SerializeField] private InputManager inputManager;

    private void Update()
    {
        TouchState touchState = inputManager.CurrentTouchState;
        for (int anglePos = 0; anglePos < 60; anglePos++)
            laneSegments[anglePos].SetActive(touchState.AnglePosPressedAtAnyDepth(anglePos));
    }
}
}
