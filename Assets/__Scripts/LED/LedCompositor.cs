using System;
using System.Collections.Generic;
using System.Linq;
using SaturnGame.UI;
using UnityEngine;
using USBIntLEDDll;

namespace SaturnGame.LED
{
    /// <summary>
    /// If there's any trouble with LED displaying, check execution order. This must be LAST!
    /// </summary>
    public class LedCompositor : PersistentSingleton<LedCompositor>
    {
        public List<LedDrawable> LedDrawableQueue;
        [SerializeField] private RingDebugManager ringDebugManager;

        [SerializeField] private Color32[] ledValues = new Color32[480]; // 60 * 8
        private LedData ledData;
        
        protected override void Awake()
        {
            base.Awake();
            ledData = new()
            {
                unitCount = 480, // 60 * 8
                rgbaValues = ledValues,
            };
        }
        
        private void FixedUpdate()
        {
            // Fill all LEDs with black first
            for (int i = 0; i < ledValues.Length; i++) ledValues[i] = Color.black;
            
            foreach (LedDrawable drawable in LedDrawableQueue.OrderBy(x => x.Layer))
                drawable.Draw(ref ledValues);
            
            LedDrawableQueue.Clear();
            
            // Send data to LED boards / debug display.
            // TODO: @cg505 fix the performance issues with LEDs in here.
            USBIntLED.Safe_USBIntLED_set(0, ledData);
            
            if (ringDebugManager == null) return;
            ringDebugManager.UpdateColors(ledValues);
        }

        private void Update()
        {
            // Toggle RingDebug when F2 is pressed
            if (Input.GetKeyDown(KeyCode.F2)) ringDebugManager.ToggleVisibility();
        }
    }
}
