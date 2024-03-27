using UnityEngine;

public class TitleScreenLogic : MonoBehaviour
{
    [SerializeField] private TitleScreenBackgroundColorRandomizer colorRandomizer;

    private void Awake()
    {
        colorRandomizer.RandomizeColor();
    }

    private static void OnConfirm()
    {
        SceneSwitcher.Instance.LoadScene("_SongSelect");
    }

    private void OnRandomizeColor()
    {
        colorRandomizer.RandomizeColor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) OnConfirm();
        if (Input.GetKeyDown(KeyCode.R)) OnRandomizeColor();
    }
}