using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Settings
{
    [System.Serializable] public class PlayerSettings
    {
        public GameSettings GameSettings = new();
        public DisplaySettings DisplaySettings = new();
        public DesignSettings DesignSettings = new();
        public SoundSettings SoundSettings = new();
    }

    [System.Serializable] public class GameSettings
    {
        public int NoteSpeed = 15;
        public int JudgementOffset;
        public int MaskDensity;
        public int BackgroundVideoSetting;
        public int BonusEffectSetting;
        public bool MirrorNotes;
        public int GiveUpSetting;
    }

    [System.Serializable] public class DisplaySettings
    {
        public int JudgementDisplayPosition;
        public bool ShowJudgementDetails;
        public int GuideLaneType;
        public int GuideLaneOpacity;
        public int DisplayOpacity;
        public bool ShowBarLines;
        public int CenterDisplayInfo;
        public int ScoreDisplayMethod;
        public bool DisplayMultiplayerRank;
        public bool DisplayStageUpEmblems;
        public bool DisplayRate;
        public bool DisplayLevel;
        public bool SkipGate;
        public bool SkipBingo;
    }

    [System.Serializable] public class DesignSettings
    {
        public int RingColor;
        public int JudgeLineColor;
        public int NoteWidth = 3;
        public int NoteColorID_Touch = 0;
        public int NoteColorID_Chain = 1;
        public int NoteColorID_SwipeClockwise = 2;
        public int NoteColorID_SwipeCounterclockwise = 3;
        public int NoteColorID_SnapForward = 4;
        public int NoteColorID_SnapBackward = 5;
        public int NoteColorID_Hold = 6;
        public bool InvertSlideColor;
        public int TouchEffect;
        public bool ShootEffect;
        public bool ShowKeyBeams;
        public bool ShowR_NoteEffect;
    }

    [System.Serializable] public class SoundSettings
    {
        public int TouchSE;
        public int BGMVolume;
        public int NavigatorMenuVolume;
        public int NavigatorPlayVolume;
        public int TouchNoteVolume;
        public int HoldNoteVolume;
        public int SlideNoteVolume;
        public int SnapNoteVolume;
        public int ChainNoteVolume;
        public int BonusEffectVolume;
        public int RNoteEffectVolume;
    }
}
