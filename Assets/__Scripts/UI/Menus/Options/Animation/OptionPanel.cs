using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SaturnGame.UI
{
    public class OptionPanel : MonoBehaviour
    {
        public RectTransform rect;
        public string Title
        {
            get => titleTMP.text;
            set { titleTMP.text = value; }
        }
        public string Subtitle
       {
            get => subtitleTMP == null ? "" : subtitleTMP.text;
            set
            { 
                if (subtitleTMP == null) return;
                subtitleTMP.text = value;
            }
        }

        [SerializeField] private TextMeshProUGUI titleTMP;
        [SerializeField] private TextMeshProUGUI subtitleTMP;
    }
}
