using UnityEngine;

public class ScoreNumRenderer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI scoreNumText;

    public void SetScoreNum(int score)
    {
        scoreNumText.text = score.ToString("0' '000' '000");
    }
}