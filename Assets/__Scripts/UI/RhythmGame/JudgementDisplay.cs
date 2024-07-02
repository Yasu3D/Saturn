using DG.Tweening;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;
using static SaturnGame.Settings.UiSettings.ShowJudgementDetailsOptions;

public class JudgementDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform GroupRect;
    [SerializeField] private CanvasGroup Group;
    [SerializeField] private GameObject TextMarvelous;
    [SerializeField] private GameObject TextGreat;
    [SerializeField] private GameObject TextGood;
    [SerializeField] private GameObject TextMiss;
    [SerializeField] private GameObject TextFast;
    [SerializeField] private GameObject TextLate;
    private Sequence currentSequence;

    private readonly Vector2 topPosition = new(0, 230);
    private readonly Vector2 middlePosition = new(0, -100);
    private readonly Vector2 bottomPosition = new(0, -320);

    public void SetDisplayPosition(int position)
    {
        GroupRect.anchoredPosition = position switch
        {
            0 => topPosition,
            1 => middlePosition,
            2 => bottomPosition,
            _ => GroupRect.anchoredPosition,
        };
    }

    public void ShowJudgement(Judgement judgement, float timeErrorMs)
    {
        currentSequence.Kill(true);

        TextMarvelous.SetActive(false);
        TextGreat.SetActive(false);
        TextGood.SetActive(false);
        TextMiss.SetActive(false);
        TextFast.SetActive(false);
        TextLate.SetActive(false);

        Group.transform.localScale = Vector3.one;

        switch (judgement)
        {
            case Judgement.None: break;

            case Judgement.Miss:
            {
                TextMiss.SetActive(true);
                break;
            }

            case Judgement.Good:
            {
                TextGood.SetActive(true);
                break;
            }

            case Judgement.Great:
            {
                TextGreat.SetActive(true);
                break;
            }

            case Judgement.Marvelous:
            {
                TextMarvelous.SetActive(true);
                break;
            }
        }

        if (SettingsManager.Instance.PlayerSettings.UiSettings.ShowJudgementDetails ==
            On && judgement is not (Judgement.Marvelous or Judgement.None))
        {
            if (timeErrorMs < 0) TextFast.SetActive(true);
            if (timeErrorMs > 0) TextLate.SetActive(true);
        }

        currentSequence = DOTween.Sequence();
        Group.transform.DOScale(0.95f, 0);

        // Miss animation is slightly different. Fades in longer.
        if (judgement is Judgement.Miss)
        {
            Group.DOFade(0, 0);
            currentSequence.Join(Group.DOFade(1, 0.15f).SetEase(Ease.OutQuad));
            currentSequence.Join(Group.transform.DOScale(1, 0.15f).SetEase(Ease.OutQuad));
        }
        else
        {
            Group.DOFade(0.8f, 0);
            currentSequence.Join(Group.DOFade(1, 0.05f).SetEase(Ease.OutQuad));
            currentSequence.Join(Group.transform.DOScale(1, 0.05f).SetEase(Ease.OutQuad));
        }

        currentSequence.Insert(0.15f, Group.DOFade(0, 0.12f).SetEase(Ease.OutQuad));
        currentSequence.Insert(0.15f, Group.transform.DOScale(0.95f, 0.12f).SetEase(Ease.OutQuad));
        currentSequence.OnComplete(() =>
        {
            TextMarvelous.SetActive(false);
            TextGreat.SetActive(false);
            TextGood.SetActive(false);
            TextMiss.SetActive(false);
            TextFast.SetActive(false);
            TextLate.SetActive(false);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4)) ShowJudgement(Judgement.Miss, 0);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ShowJudgement(Judgement.Good, 5);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ShowJudgement(Judgement.Great, -2);
        if (Input.GetKeyDown(KeyCode.Alpha1)) ShowJudgement(Judgement.Marvelous, 20);

        if (Input.GetKeyDown(KeyCode.Alpha5)) SetDisplayPosition(0);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetDisplayPosition(1);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SetDisplayPosition(2);
    }
}
