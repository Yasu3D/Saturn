namespace SaturnGame.Settings
{
[System.Serializable]
public class PlayerSettings
{
    public GameSettings GameSettings = new();
    public UISettings UISettings = new();
    public DesignSettings DesignSettings = new();
    public SoundSettings SoundSettings = new();

    public void SetParameter(string parameter, int value)
    {
        switch (parameter)
        {
            case "NoteSpeed":
            {
                GameSettings.NoteSpeed = value;
                break;
            }
            case "JudgementOffset":
            {
                GameSettings.JudgementOffset = value;
                break;
            }
            case "MaskDensity":
            {
                GameSettings.MaskDensity = value;
                break;
            }
            case "BackgroundVideoSetting":
            {
                GameSettings.BackgroundVideoSetting = value;
                break;
            }
            case "BonusEffectSetting":
            {
                GameSettings.BonusEffectSetting = value;
                break;
            }
            case "MirrorNotes":
            {
                GameSettings.MirrorNotes = value;
                break;
            }
            case "GiveUpSetting":
            {
                GameSettings.GiveUpSetting = value;
                break;
            }

            case "JudgementDisplayPosition":
            {
                UISettings.JudgementDisplayPosition = value;
                break;
            }
            case "ShowJudgementDetails":
            {
                UISettings.ShowJudgementDetails = value;
                break;
            }
            case "GuideLaneType":
            {
                UISettings.GuideLaneType = value;
                break;
            }
            case "GuideLaneOpacity":
            {
                UISettings.GuideLaneOpacity = value;
                break;
            }
            case "DisplayOpacity":
            {
                UISettings.DisplayOpacity = value;
                break;
            }
            case "ShowBarLines":
            {
                UISettings.ShowBarLines = value;
                break;
            }
            case "CenterDisplayInfo":
            {
                UISettings.CenterDisplayInfo = value;
                break;
            }
            case "ScoreDisplayMethod":
            {
                UISettings.ScoreDisplayMethod = value;
                break;
            }

            case "RingColor":
            {
                DesignSettings.RingColor = value;
                break;
            }
            case "JudgeLineColor":
            {
                DesignSettings.JudgeLineColor = value;
                break;
            }
            case "NoteWidth":
            {
                DesignSettings.NoteWidth = value;
                break;
            }
            case "NoteColorID_Touch":
            {
                DesignSettings.NoteColorIDTouch = value;
                break;
            }
            case "NoteColorID_Chain":
            {
                DesignSettings.NoteColorIDChain = value;
                break;
            }
            case "NoteColorID_SwipeClockwise":
            {
                DesignSettings.NoteColorIDSwipeClockwise = value;
                break;
            }
            case "NoteColorID_SwipeCounterclockwise":
            {
                DesignSettings.NoteColorIDSwipeCounterclockwise = value;
                break;
            }
            case "NoteColorID_SnapForward":
            {
                DesignSettings.NoteColorIDSnapForward = value;
                break;
            }
            case "NoteColorID_SnapBackward":
            {
                DesignSettings.NoteColorIDSnapBackward = value;
                break;
            }
            case "NoteColorID_Hold":
            {
                DesignSettings.NoteColorIDHold = value;
                break;
            }
            case "InvertSlideColor":
            {
                DesignSettings.InvertSlideColor = value;
                break;
            }
            case "TouchEffect":
            {
                DesignSettings.TouchEffect = value;
                break;
            }
            case "ShowShootEffect":
            {
                DesignSettings.ShowShootEffect = value;
                break;
            }
            case "ShowKeyBeams":
            {
                DesignSettings.ShowKeyBeams = value;
                break;
            }
            case "ShowRNoteEffect":
            {
                DesignSettings.ShowRNoteEffect = value;
                break;
            }

            case "TouchSE":
            {
                SoundSettings.TouchSE = value;
                break;
            }
            case "BGMVolume":
            {
                SoundSettings.BGMVolume = value;
                break;
            }
            case "HitsoundOverallVolume":
            {
                SoundSettings.HitsoundOverallVolume = value;
                break;
            }
            case "GuideVolume":
            {
                SoundSettings.GuideVolume = value;
                break;
            }
            case "TouchNoteVolume":
            {
                SoundSettings.TouchNoteVolume = value;
                break;
            }
            case "HoldNoteVolume":
            {
                SoundSettings.HoldNoteVolume = value;
                break;
            }
            case "SlideNoteVolume":
            {
                SoundSettings.SlideNoteVolume = value;
                break;
            }
            case "SnapNoteVolume":
            {
                SoundSettings.SnapNoteVolume = value;
                break;
            }
            case "ChainNoteVolume":
            {
                SoundSettings.ChainNoteVolume = value;
                break;
            }
            case "BonusEffectVolume":
            {
                SoundSettings.BonusEffectVolume = value;
                break;
            }
            case "RNoteEffectVolume":
            {
                SoundSettings.RNoteEffectVolume = value;
                break;
            }
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
            "NoteColorID_Touch" => DesignSettings.NoteColorIDTouch,
            "NoteColorID_Chain" => DesignSettings.NoteColorIDChain,
            "NoteColorID_SwipeClockwise" => DesignSettings.NoteColorIDSwipeClockwise,
            "NoteColorID_SwipeCounterclockwise" => DesignSettings.NoteColorIDSwipeCounterclockwise,
            "NoteColorID_SnapForward" => DesignSettings.NoteColorIDSnapForward,
            "NoteColorID_SnapBackward" => DesignSettings.NoteColorIDSnapBackward,
            "NoteColorID_Hold" => DesignSettings.NoteColorIDHold,
            "InvertSlideColor" => DesignSettings.InvertSlideColor,
            "TouchEffect" => DesignSettings.TouchEffect,
            "ShowShootEffect" => DesignSettings.ShowShootEffect,
            "ShowKeyBeams" => DesignSettings.ShowKeyBeams,
            "ShowRNoteEffect" => DesignSettings.ShowRNoteEffect,
            "TouchSE" => SoundSettings.TouchSE,
            "BGMVolume" => SoundSettings.BGMVolume,
            "HitsoundOverallVolume" => SoundSettings.HitsoundOverallVolume,
            "GuideVolume" => SoundSettings.GuideVolume,
            "TouchNoteVolume" => SoundSettings.TouchNoteVolume,
            "HoldNoteVolume" => SoundSettings.HoldNoteVolume,
            "SlideNoteVolume" => SoundSettings.SlideNoteVolume,
            "SnapNoteVolume" => SoundSettings.SnapNoteVolume,
            "ChainNoteVolume" => SoundSettings.ChainNoteVolume,
            "BonusEffectVolume" => SoundSettings.BonusEffectVolume,
            "RNoteEffectVolume" => SoundSettings.RNoteEffectVolume,
            _ => 0,
        };
    }
}

// ReSharper disable RedundantDefaultMemberInitializer
[System.Serializable]
public class GameSettings
{
    public int NoteSpeed = 25;
    public int JudgementOffset = 0;
    public int MaskDensity = 2;
    public int BackgroundVideoSetting = 0;
    public int BonusEffectSetting = 1;
    public int MirrorNotes = 0;
    public int GiveUpSetting = 0;
}

[System.Serializable]
public class UISettings
{
    public int JudgementDisplayPosition = 0;
    public int ShowJudgementDetails = 1;
    public int GuideLaneType = 1;
    public int GuideLaneOpacity = 5;
    public int DisplayOpacity = 3;
    public int ShowBarLines = 1;
    public int CenterDisplayInfo = 0;
    public int ScoreDisplayMethod = 0;
}

[System.Serializable]
public class DesignSettings
{
    public int RingColor = 0;
    public int JudgeLineColor = 2;
    public int NoteWidth = 3;
    public int NoteColorIDTouch = 0;
    public int NoteColorIDChain = 1;
    public int NoteColorIDSwipeClockwise = 2;
    public int NoteColorIDSwipeCounterclockwise = 3;
    public int NoteColorIDSnapForward = 4;
    public int NoteColorIDSnapBackward = 5;
    public int NoteColorIDHold = 6;
    public int InvertSlideColor = 0;
    public int TouchEffect = 1;
    public int ShowShootEffect = 1;
    public int ShowKeyBeams = 1;
    public int ShowRNoteEffect = 1;
}

[System.Serializable]
public class SoundSettings
{
    public int TouchSE = 0;
    public int BGMVolume = 100;
    public int HitsoundOverallVolume = 70;
    public int GuideVolume = 30;
    public int TouchNoteVolume = 80;
    public int HoldNoteVolume = 80;
    public int SlideNoteVolume = 80;
    public int SnapNoteVolume = 80;
    public int ChainNoteVolume = 80;
    public int BonusEffectVolume = 80;
    public int RNoteEffectVolume = 80;
}
// ReSharper restore RedundantDefaultMemberInitializer
}