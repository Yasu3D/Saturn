using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame
{
    public class BossBackgroundAnimator : MonoBehaviour
    {
        [SerializeField] private Transform tunnel;
        [SerializeField] private float scrollSpeed = 10;
        
        private void Update()
        {
            tunnel.localPosition += Vector3.forward * (scrollSpeed * Time.deltaTime);
            if (tunnel.localPosition.z >= 60) tunnel.localPosition = Vector3.zero;
        }
    }
}
