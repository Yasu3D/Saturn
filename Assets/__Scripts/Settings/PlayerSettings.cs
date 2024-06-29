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
            case "OffsetMode":
            {
                GameSettings.OffsetMode = value;
                break;
            }
            case "AudioOffset":
            {
                GameSettings.AudioOffset = value;
                break;
            }
            case "VisualOffset":
            {
                GameSettings.VisualOffset = value;
                break;
            }
            case "InputLatency":
            {
                GameSettings.InputLatency = value;
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
            "OffsetMode" => GameSettings.OffsetMode,
            "AudioOffset" => GameSettings.AudioOffset,
            "VisualOffset" => GameSettings.VisualOffset,
            "InputLatency" => GameSettings.InputLatency,
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
    /// <summary>
    /// Note Speed from 10 [1.0] to 60 [6.0]
    /// </summary>
    public int NoteSpeed = 25;

    /// <summary>
    /// 0 > Standard / Low Latency
    /// 1 > Classic / Original Latency
    /// 2 > Advanced
    /// </summary>
    public int OffsetMode = 0;
    
    /// <summary>
    /// Audio Offset from +100 [10] to -100 [-10]
    /// TODO: Expand to +200 -200
    /// </summary>
    public int AudioOffset = 0;
    
    /// <summary>
    /// Visual Offset from +100 [10] to -100 [-10]
    /// TODO: Expand to +200 -200
    /// </summary>
    public int VisualOffset = 0;

    /// <summary>
    /// Input latency. No clue about range yet.
    /// TODO: @cg505 define range for this.
    /// </summary>
    public int InputLatency = 0;
    
    /// <summary>
    /// Mask Density from 0 to +4
    /// </summary>
    public int MaskDensity = 2;
    
    /// <summary>
    /// 0 > Ask<br/>
    /// 1 > Off<br/>
    /// 2 > On<br/>
    /// </summary>
    public int BackgroundVideoSetting = 0;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int BonusEffectSetting = 1;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int MirrorNotes = 0;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > No Touch<br/>
    /// 2 > S Border<br/>
    /// 3 > SS Border<br/>
    /// 4 > SSS Border<br/>
    /// 5 > Personal Best Border<br/>
    /// </summary>
    public int GiveUpSetting = 0;
}

[System.Serializable]
public class UISettings
{
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > Top<br/>
    /// 2 > Center<br/>
    /// 3 > Bottom<br/>
    /// </summary>
    public int JudgementDisplayPosition = 2;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int ShowJudgementDetails = 1;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > A<br/>
    /// 2 > B<br/>
    /// 3 > C<br/>
    /// 4 > D<br/>
    /// 5 > E<br/>
    /// 6 > F<br/>
    /// 7 > G<br/>
    /// </summary>
    public int GuideLaneType = 1;
    
    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int GuideLaneOpacity = 5;
    
    /// <summary>
    /// 0 >   0%<br/>
    /// 1 >  20%<br/>
    /// 2 >  40%<br/>
    /// 3 >  60%<br/>
    /// 4 >  80%<br/>
    /// 5 > 100%<br/>
    /// </summary>
    public int DisplayOpacity = 5;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > On<br/>
    /// </summary>
    public int ShowBarLines = 1;
    
    /// <summary>
    /// 0 > Off<br/>
    /// 1 > Combo<br/>
    /// 2 > Plus Method<br/>
    /// 3 > Minus Method<br/>
    /// 4 > Average Method<br/>
    /// 5 > S Border<br/>
    /// 6 > SS Border<br/>
    /// 7 > SSS Border<br/>
    /// 8 > Personal Best Border<br/>
    /// </summary>
    public int CenterDisplayInfo = 1;
    
    /// <summary>
    /// 0 > Plus Method<br/>
    /// 1 > Minus Method<br/>
    /// 2 > Average Method<br/>
    /// </summary>
    public int ScoreDisplayMethod = 0;
}

[System.Serializable]
public class DesignSettings
{
    /// <summary>
    /// WIP
    /// </summary>
    public int RingColor = 0;
    
    /// <summary>
    /// 0 > Original<br/>
    /// 1 > Lily<br/>
    /// 2 > Reverse<br/>
    /// </summary>
    public int JudgeLineColor = 2;
    
    /// <summary>
    /// 1 > 1
    /// 2 > 2
    /// 3 > 3
    /// 4 > 4
    /// 5 > 5
    /// </summary>
    public int NoteWidth = 3;
    
    
    public int NoteColorIDTouch = 0;
    public int NoteColorIDChain = 1;
    public int NoteColorIDSwipeClockwise = 2;
    public int NoteColorIDSwipeCounterclockwise = 3;
    public int NoteColorIDSnapForward = 4;
    public int NoteColorIDSnapBackward = 5;
    public int NoteColorIDHold = 6;
    
    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int InvertSlideColor = 0;
    
    /// <summary>
    /// WIP
    /// </summary>
    public int TouchEffect = 1;
    
    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int ShowShootEffect = 1;
    
    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
    public int ShowKeyBeams = 1;
    
    /// <summary>
    /// 0 > OFF
    /// 1 > ON
    /// </summary>
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