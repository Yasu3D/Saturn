using UnityEngine;
using TMPro;

namespace SaturnGame.UI
{
    public class TextTicker : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float scrollSpeed = -75;
        [SerializeField] private float textSpacing = 50;

        private TextMeshProUGUI cloneText;
        private RectTransform cloneRect;

        private bool enableScroll;
        private float startPos;
        private float textBounds;
        private float offset;

        void Awake()
        {
            // Instantiate and parent a copy of the original text object.
            startPos = rect.anchoredPosition.x;
            cloneText = Instantiate(text);
            cloneRect = cloneText.rectTransform;
            cloneRect.SetParent(rect);

            UpdateComponents();
        }

        void UpdateComponents()
        {
            // Recalculate values
            textBounds = text.GetPreferredValues().x + textSpacing;
            enableScroll = rect.rect.width < textBounds;
            
            // Reset offset and text positions.
            offset = 0;
            rect.anchoredPosition = new(startPos + offset, 0);
            cloneRect.anchoredPosition = rect.anchoredPosition + new Vector2(textBounds, 0);
            
            // Update cloned text and set visibility
            cloneText.text = text.text;
            cloneText.gameObject.SetActive(enableScroll);
        }

        void Update()
        {
            if (text.havePropertiesChanged) UpdateComponents();
            
            if (enableScroll)
            {
                offset += scrollSpeed * Time.deltaTime;
                offset %= textBounds;
            }
            else offset = 0;

            rect.anchoredPosition = new(startPos + offset, 0);
        }
    }
}