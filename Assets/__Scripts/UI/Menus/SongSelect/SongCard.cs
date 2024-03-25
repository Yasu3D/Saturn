using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
    public class SongCard : MonoBehaviour
    {
        [FormerlySerializedAs("rect")] public RectTransform Rect;
        [FormerlySerializedAs("titleText")] public TextMeshProUGUI TitleText;
        [FormerlySerializedAs("artistText")] public TextMeshProUGUI ArtistText;
        [FormerlySerializedAs("difficultyText")] public TextMeshProUGUI DifficultyText;
        [FormerlySerializedAs("jacketImage")] public RawImage JacketImage;
    }
}
