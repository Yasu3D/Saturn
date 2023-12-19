using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.Settings
{
    [System.Serializable] public class PlayerSettings
    {
        public GameSettings GameSettings = new();
        public UISettings UISettings = new();
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

    [System.Serializable] public class UISettings
    {
        public int JudgementDisplayPosition;
        public bool ShowJudgementDetails;
        public int GuideLaneType = 1;
        public int GuideLaneOpacity = 5;
        public int DisplayOpacity;
        public bool ShowBarLines = true;
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
        public int JudgeLineColor = 2;
        public int NoteWidth = 3;
        public int NoteColorID_Touch = 0;
        public int NoteColorID_Chain = 1;
        public int NoteColorID_SwipeClockwise = 2;
        public int NoteColorID_SwipeCounterclockwise = 3;
        public int NoteColorID_SnapForward = 4;
        public int NoteColorID_SnapBackward = 5;
        public int NoteColorID_Hold = 6;
        public bool InvertSlideColor = false;
        public int TouchEffect;
        public bool ShootEffect = true;
        public bool ShowKeyBeams = true;
        public bool ShowR_NoteEffect = true;
    }

    [System.Serializable] public class SoundSettings
    {
        public int TouchSE;
        public int BGMVolume = 100;
        public int NavigatorMenuVolume = 30;
        public bool NavigatorDuringPlay;
        public int TouchNoteVolume = 80;
        public int HoldNoteVolume = 80;
        public int SlideNoteVolume = 80;
        public int SnapNoteVolume = 80;
        public int ChainNoteVolume = 80;
        public int BonusEffectVolume = 80;
        public int RNoteEffectVolume = 80;
    }
}
