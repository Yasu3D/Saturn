using UnityEngine;
using TMPro;
using System;
using System.Globalization;

public class DateTimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dateTimeTMP;

    private void Update()
    {
        DateTime localDate = DateTime.Now;
        dateTimeTMP.text = localDate.ToString(new CultureInfo("ja-JP"));
    }
}