using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaturnGame.UI
{
    public class SongCard : MonoBehaviour
    {
        public bool testSwitch;

        [Header("Data")]
        public string title;
        public string artist;
        public string difficulty;

        [Header("Objects")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI artistText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private Image jacket;

        [Header("UI Elements")]
        [SerializeField] private RectTransform containerRect;
        [SerializeField] private RectTransform shadowRect;
        [SerializeField] private RectTransform dataRect;

        void Update()
        {
            if (testSwitch)
            {
                containerRect.sizeDelta = new(containerRect.rect.width, containerRect.rect.width);
                shadowRect.sizeDelta = new(shadowRect.rect.width, shadowRect.rect.width);
                dataRect.gameObject.SetActive(false);
            }
            else
            {
                containerRect.sizeDelta = new(containerRect.rect.width, 390);
                shadowRect.sizeDelta = new(shadowRect.rect.width, 420);
                dataRect.gameObject.SetActive(true);
            }
        }
    }
}
