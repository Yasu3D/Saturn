using SaturnGame.UI;
using TMPro;
using UnityEngine;

public class SongTitleText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ArcTextMeshPro arc;
    private void Start()
    {
        UpdateText(PersistentStateManager.Instance.SelectedSong.Title);
    }

    private void UpdateText(string title)
    {
        text.text = title;
        arc.UpdateText();
    }
}