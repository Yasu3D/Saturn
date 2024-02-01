using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenLogic : MonoBehaviour
{
    [SerializeField] private TitleScreenBackgroundColorRandomizer colorRandomizer;

    void Awake()
    {
        colorRandomizer.RandomizeColor();
    }

    public void OnConfirm()
    {
        SceneSwitcher.Instance.LoadScene("_SongSelect");
    }

    public void OnRandomizeColor()
    {
        colorRandomizer.RandomizeColor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
        if (Input.GetKeyDown(KeyCode.R)) OnRandomizeColor();
    }
}
