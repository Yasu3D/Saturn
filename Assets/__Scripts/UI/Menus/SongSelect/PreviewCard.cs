using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
    public class PreviewCard : MonoBehaviour
    {
        [FormerlySerializedAs("rect")] public RectTransform Rect;
        [FormerlySerializedAs("jacketImage")] public RawImage JacketImage;
    }
}
