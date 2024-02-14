using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace SaturnGame.Settings
{
    [System.Serializable] public class PlayerSettings
    {
        public GameSettings GameSettings = new();
        public UISettings UISettings = new();
        public DesignSettings DesignSettings = new();
        public SoundSettings SoundSettings = new();

        public void SetParameter(string parameter, int value)
        {
            switch (parameter)
            {
                case "NoteSpeed": GameSettings.NoteSpeed = value; break;
                case "JudgementOffset": GameSettings.JudgementOffset = value; break;
                case "MaskDensity": GameSettings.MaskDensity = value; break;
                case "BackgroundVideoSetting": GameSettings.BackgroundVideoSetting = value; break;
                case "BonusEffectSetting": GameSettings.BonusEffectSetting = value; break;
                case "MirrorNotes": GameSettings.MirrorNotes = value; break;
                case "GiveUpSetting": GameSettings.GiveUpSetting = value; break;

                case "JudgementDisplayPosition": UISettings.JudgementDisplayPosition = value; break;
                case "ShowJudgementDetails": UISettings.ShowJudgementDetails = value; break;
                case "GuideLaneType": UISettings.GuideLaneType = value; break;
                case "GuideLaneOpacity": UISettings.GuideLaneOpacity = value; break;
                case "DisplayOpacity": UISettings.DisplayOpacity = value; break;
                case "ShowBarLines": UISettings.ShowBarLines = value; break;
                case "CenterDisplayInfo": UISettings.CenterDisplayInfo = value; break;
                case "ScoreDisplayMethod": UISettings.ScoreDisplayMethod = value; break;

                case "RingColor": DesignSettings.RingColor = value; break;
                case "JudgeLineColor": DesignSettings.JudgeLineColor = value; break;
                case "NoteWidth": DesignSettings.NoteWidth = value; break;
                case "NoteColorID_Touch": DesignSettings.NoteColorID_Touch = value; break;
                case "NoteColorID_Chain": DesignSettings.NoteColorID_Chain = value; break;
                case "NoteColorID_SwipeClockwise": DesignSettings.NoteColorID_SwipeClockwise = value; break;
                case "NoteColorID_SwipeCounterclockwise": DesignSettings.NoteColorID_SwipeCounterclockwise = value; break;
                case "NoteColorID_SnapForward": DesignSettings.NoteColorID_SnapForward = value; break;
                case "NoteColorID_SnapBackward": DesignSettings.NoteColorID_SnapBackward = value; break;
                case "NoteColorID_Hold": DesignSettings.NoteColorID_Hold = value; break;
                case "InvertSlideColor": DesignSettings.InvertSlideColor = value; break;
                case "TouchEffect": DesignSettings.TouchEffect = value; break;
                case "ShootEffect": DesignSettings.ShootEffect = value; break;
                case "ShowKeyBeams": DesignSettings.ShowKeyBeams = value; break;
                case "ShowR_NoteEffect": DesignSettings.ShowR_NoteEffect = value; break;

                case "TouchSE": SoundSettings.TouchSE = value; break;
                case "BGMVolume": SoundSettings.BGMVolume = value; break;
                case "GuideVolume": SoundSettings.GuideVolume = value; break;
                case "TouchNoteVolume": SoundSettings.TouchNoteVolume = value; break;
                case "HoldNoteVolume": SoundSettings.HoldNoteVolume = value; break;
                case "SlideNoteVolume": SoundSettings.SlideNoteVolume = value; break;
                case "SnapNoteVolume": SoundSettings.SnapNoteVolume = value; break;
                case "ChainNoteVolume": SoundSettings.ChainNoteVolume = value; break;
                case "BonusEffectVolume": SoundSettings.BonusEffectVolume = value; break;
                case "RNoteEffectVolume": SoundSettings.RNoteEffectVolume = value; break;
            }
        }

        public int GetParameter(string parameter)
        {
            return parameter switch
            {
                "NoteSpeed" => GameSettings.NoteSpeed,
                "JudgementOffset" => GameSettings.JudgementOffset,
                "MaskDensity" => GameSettings.MaskDensity,
                "BackgroundVideoSetting" => GameSettings.BackgroundVideoSetting,
                "BonusEffectSetting" => GameSettings.BonusEffectSetting,
                "MirrorNotes" => GameSettings.MirrorNotes,
                "GiveUpSetting" => GameSettings.GiveUpSetting,
                "JudgementDisplayPosition" => UISettings.JudgementDisplayPosition,
                "ShowJudgementDetails" => UISettings.ShowJudgementDetails,
                "GuideLaneType" => UISettings.GuideLaneType,
                "GuideLaneOpacity" => UISettings.GuideLaneOpacity,
                "DisplayOpacity" => UISettings.DisplayOpacity,
                "ShowBarLines" => UISettings.ShowBarLines,
                "CenterDisplayInfo" => UISettings.CenterDisplayInfo,
                "ScoreDisplayMethod" => UISettings.ScoreDisplayMethod,
                "RingColor" => DesignSettings.RingColor,
                "JudgeLineColor" => DesignSettings.JudgeLineColor,
                "NoteWidth" => DesignSettings.NoteWidth,
                "NoteColorID_Touch" => DesignSettings.NoteColorID_Touch,
                "NoteColorID_Chain" => DesignSettings.NoteColorID_Chain,
                "NoteColorID_SwipeClockwise" => DesignSettings.NoteColorID_SwipeClockwise,
                "NoteColorID_SwipeCounterclockwise" => DesignSettings.NoteColorID_SwipeCounterclockwise,
                "NoteColorID_SnapForward" => DesignSettings.NoteColorID_SnapForward,
                "NoteColorID_SnapBackward" => DesignSettings.NoteColorID_SnapBackward,
                "NoteColorID_Hold" => DesignSettings.NoteColorID_Hold,
                "InvertSlideColor" => DesignSettings.InvertSlideColor,
                "TouchEffect" => DesignSettings.TouchEffect,
                "ShootEffect" => DesignSettings.ShootEffect,
                "ShowKeyBeams" => DesignSettings.ShowKeyBeams,
                "ShowR_NoteEffect" => DesignSettings.ShowR_NoteEffect,
                "TouchSE" => SoundSettings.TouchSE,
                "BGMVolume" => SoundSettings.BGMVolume,
                "NavigatorMenuVolume" => SoundSettings.NavigatorMenuVolume,
                "TouchNoteVolume" => SoundSettings.TouchNoteVolume,
                "HoldNoteVolume" => SoundSettings.HoldNoteVolume,
                "SlideNoteVolume" => SoundSettings.SlideNoteVolume,
                "SnapNoteVolume" => SoundSettings.SnapNoteVolume,
                "ChainNoteVolume" => SoundSettings.ChainNoteVolume,
                "BonusEffectVolume" => SoundSettings.BonusEffectVolume,
                "RNoteEffectVolume" => SoundSettings.RNoteEffectVolume,
                _ => -1,
            };
        }
    }

    [System.Serializable] public class GameSettings
    {
        public int NoteSpeed = 15;
        public int JudgementOffset;
        public int MaskDensity;
        public int BackgroundVideoSetting;
        public int BonusEffectSetting;
        public int MirrorNotes;
        public int GiveUpSetting;
    }

    [System.Serializable] public class UISettings
    {
        public int JudgementDisplayPosition;
        public int ShowJudgementDetails;
        public int GuideLaneType = 1;
        public int GuideLaneOpacity = 5;
        public int DisplayOpacity;
        public int ShowBarLines = 1;
        public int CenterDisplayInfo;
        public int ScoreDisplayMethod;
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
        public int InvertSlideColor;
        public int TouchEffect;
        public int ShootEffect = 1;
        public int ShowKeyBeams = 1;
        public int ShowR_NoteEffect = 1;
    }

    [System.Serializable] public class SoundSettings
    {
        public int TouchSE;
        public int BGMVolume = 100;
        public int GuideVolume = 30;
        public int TouchNoteVolume = 80;
        public int HoldNoteVolume = 80;
        public int SlideNoteVolume = 80;
        public int SnapNoteVolume = 80;
        public int ChainNoteVolume = 80;
        public int BonusEffectVolume = 80;
        public int RNoteEffectVolume = 80;
    }
}
