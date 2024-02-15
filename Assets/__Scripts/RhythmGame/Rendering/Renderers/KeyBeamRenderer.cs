using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaturnGame.RhythmGame;

namespace SaturnGame.Rendering
{
    public class KeyBeamRenderer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> laneSegments;
        [SerializeField] private InputManager inputManager;

        void Update()
        {
            TouchState touchState = inputManager.CurrentTouchState;
            if (touchState is null) return;

            foreach (int anglePos in Enumerable.Range(0, 60)) {
                laneSegments[anglePos].SetActive(touchState.AnglePosPressedAtAnyDepth(anglePos));
            }
        }
    }

}
