using System.Collections;
using System.Collections.Generic;
using SaturnGame.RhythmGame;
using UnityEngine;

namespace SaturnGame
{
public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private ChartManager chartManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Material backgroundOrbParticleMaterial;
    
    private static readonly int ParticleBpmProperty = Shader.PropertyToID("_BPM");
    private static readonly int ParticleVisualTimeProperty = Shader.PropertyToID("_VisualTime");
        
    private Chart Chart => chartManager.Chart;
    private int bgmDataIndex;
    
    private void UpdateOrbParticles()
    {
        if (backgroundOrbParticleMaterial.HasFloat(ParticleVisualTimeProperty))
            backgroundOrbParticleMaterial.SetFloat(ParticleVisualTimeProperty, timeManager.VisualTimeMs);
        
        if (bgmDataIndex > Chart.Notes.Count - 1) return;

        while (bgmDataIndex < Chart.BGMDataGimmicks.Count && Chart.BGMDataGimmicks[bgmDataIndex].TimeMs <= timeManager.VisualTimeMs)
        {
            if (backgroundOrbParticleMaterial.HasFloat(ParticleBpmProperty))
                backgroundOrbParticleMaterial.SetFloat(ParticleBpmProperty, Chart.BGMDataGimmicks[bgmDataIndex].BeatsPerMinute);
            
            bgmDataIndex++;
        }
    }

    private void Update()
    {
        UpdateOrbParticles();
    }
}
}