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
    
    [SerializeField] private ParticleSystemRenderer backgroundOrbParticleSystem;
    [SerializeField] private MeshRenderer bossSquares;
    private Material standardParticleMaterialInstance;
    private Material bossSquaresMaterialInstance;
    
    private static readonly int BpmProperty = Shader.PropertyToID("_BPM");
    private static readonly int VisualTimeProperty = Shader.PropertyToID("_VisualTime");
        
    private Chart Chart => chartManager.Chart;
    private int bgmDataIndex;

    private void Awake()
    {
        standardParticleMaterialInstance = new(backgroundOrbParticleSystem.material);
        backgroundOrbParticleSystem.material = standardParticleMaterialInstance;

        bossSquaresMaterialInstance = new(bossSquares.material);
        bossSquares.material = bossSquaresMaterialInstance;
    }
    
    private void Update()
    {
        UpdateBackgroundShaders();
    }
    
    private void UpdateBackgroundShaders()
    {
        if (standardParticleMaterialInstance.HasFloat(VisualTimeProperty))
            standardParticleMaterialInstance.SetFloat(VisualTimeProperty, timeManager.VisualTimeMs);
        
        if (bossSquaresMaterialInstance.HasFloat(VisualTimeProperty))
            bossSquaresMaterialInstance.SetFloat(VisualTimeProperty, timeManager.VisualTimeMs);
        
        if (bgmDataIndex > Chart.Notes.Count - 1) return;

        while (bgmDataIndex < Chart.BGMDataGimmicks.Count && Chart.BGMDataGimmicks[bgmDataIndex].TimeMs <= timeManager.VisualTimeMs)
        {
            if (standardParticleMaterialInstance.HasFloat(BpmProperty))
                standardParticleMaterialInstance.SetFloat(BpmProperty, Chart.BGMDataGimmicks[bgmDataIndex].BeatsPerMinute);
            
            if (bossSquaresMaterialInstance.HasFloat(BpmProperty))
                bossSquaresMaterialInstance.SetFloat(BpmProperty, Chart.BGMDataGimmicks[bgmDataIndex].BeatsPerMinute);
            
            bgmDataIndex++;
        }
    }
}
}