using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SaturnGame.LED;
using UnityEngine;

namespace SaturnGame
{
    public class TouchRipplePool : MonoBehaviourPool<TouchRippleDrawable>
    {
        [SerializeField] private string scaPath;
        private ColorArray colorArray;
        public List<ScaFrame> Frames => colorArray.Frames;
        
        private void Awake()
        {
            colorArray = ColorArray.ParseWithSize(Path.Combine(Application.streamingAssetsPath, scaPath), 14, 9);
        }
    }
}
