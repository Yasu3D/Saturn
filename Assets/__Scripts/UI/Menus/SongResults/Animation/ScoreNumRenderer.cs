using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreNumRenderer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI scoreNumText;

    public void SetScoreNum(int score)
    {
        // TODO spaces, padding
        scoreNumText.text = score.ToString();
    }
}
