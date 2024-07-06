using DG.Tweening;
using SaturnGame.RhythmGame;
using SaturnGame.Settings;
using UnityEngine;
using static SaturnGame.Settings.UiSettings.ShowJudgementDetailsOptions;

public class JudgementDisplay : MonoBehaviour
{
    [SerializeField] private RectTransform groupRect;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject textMarvelous;
    [SerializeField] private GameObject textGreat;
    [SerializeField] private GameObject textGood;
    [SerializeField] private GameObject textMiss;
    [SerializeField] private GameObject textFast;
    [SerializeField] private GameObject textLate;
    private Sequence currentSequence;

    private readonly Vector2 topPosition = new(0, 215);
    private readonly Vector2 middlePosition = new(0, -114);
    private readonly Vector2 bottomPosition = new(0, -333);

    public void SetDisplayPosition(int position)
    {
        groupRect.anchoredPosition = position switch
        {
            0 => topPosition,
            1 => middlePosition,
            2 => bottomPosition,
            _ => groupRect.anchoredPosition,
        };
    }

    public void ShowJudgement(Judgement judgement, float timeErrorMs)
    {
        currentSequence.Kill(true);

        textMarvelous.SetActive(false);
        textGreat.SetActive(false);
        textGood.SetActive(false);
        textMiss.SetActive(false);
        textFast.SetActive(false);
        textLate.SetActive(false);

        group.transform.localScale = Vector3.one;

        switch (judgement)
        {
            case Judgement.None: break;

            case Judgement.Miss:
            {
                textMiss.SetActive(true);
                break;
            }

            case Judgement.Good:
            {
                textGood.SetActive(true);
                break;
            }

            case Judgement.Great:
            {
                textGreat.SetActive(true);
                break;
            }

            case Judgement.Marvelous:
            {
                textMarvelous.SetActive(true);
                break;
            }
        }

        if (SettingsManager.Instance.PlayerSettings.UiSettings.ShowJudgementDetails ==
            On && judgement is not (Judgement.Marvelous or Judgement.None))
        {
            if (timeErrorMs < 0) textFast.SetActive(true);
            if (timeErrorMs > 0) textLate.SetActive(true);
        }

        currentSequence = DOTween.Sequence();
        group.transform.DOScale(0.95f, 0);

        // Miss animation is slightly different. Fades in longer.
        if (judgement is Judgement.Miss)
        {
            group.DOFade(0, 0);
            currentSequence.Join(group.DOFade(1, 0.15f).SetEase(Ease.OutQuad));
            currentSequence.Join(group.transform.DOScale(1, 0.15f).SetEase(Ease.OutQuad));
        }
        else
        {
            group.DOFade(0.8f, 0);
            currentSequence.Join(group.DOFade(1, 0.05f).SetEase(Ease.OutQuad));
            currentSequence.Join(group.transform.DOScale(1, 0.05f).SetEase(Ease.OutQuad));
        }

        currentSequence.Insert(0.15f, group.DOFade(0, 0.12f).SetEase(Ease.OutQuad));
        currentSequence.Insert(0.15f, group.transform.DOScale(0.95f, 0.12f).SetEase(Ease.OutQuad));
        currentSequence.OnComplete(() =>
        {
            textMarvelous.SetActive(false);
            textGreat.SetActive(false);
            textGood.SetActive(false);
            textMiss.SetActive(false);
            textFast.SetActive(false);
            textLate.SetActive(false);
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
