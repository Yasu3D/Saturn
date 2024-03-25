using UnityEngine;
using UnityEngine.UI;

public class TitleScreenBackgroundColorRandomizer : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image checkers;

    public void RandomizeColor()
    {
        float bgHue = Random.value;
        float checkerHue = (bgHue + 0.111111f) % 1;

        Color bgColor, checkerColor;

        // Super secret and cool awesome greyscale title screen :O
        if (bgHue < 0.01f)
        {
            bgColor = new Color(0.32f, 0.32f, 0.32f, 1.00f);
            checkerColor = new Color(1.00f, 1.00f, 1.00f, 0.12f);
        }
        else
        {
            bgColor = Color.HSVToRGB(bgHue, 0.62f, 0.43f, false);
            checkerColor = Color.HSVToRGB(checkerHue, 0.54f, 1, false);
            checkerColor.a = 0.12f;
        }

        background.color = bgColor;
        checkers.color = checkerColor;
    }
}
