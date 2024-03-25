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

        private string prevText = "";
        private bool enableScroll;
        private float startPos;
        private float textBounds;
        private float offset;

        private void Awake()
        {
            // Instantiate and parent a copy of the original text object.
            startPos = rect.anchoredPosition.x;
            cloneText = Instantiate(text);
            cloneRect = cloneText.rectTransform;
            cloneRect.localScale = Vector3.one;
            cloneRect.SetParent(rect);

            UpdateComponents();
        }

        private void UpdateComponents()
        {
            prevText = text.text;

            // Recalculate values
            textBounds = text.GetPreferredValues().x + textSpacing;
            enableScroll = rect.rect.width < textBounds;

            // Reset offset and text positions.
            offset = 0;
            rect.anchoredPosition = new Vector2(startPos + offset, 0);
            cloneRect.anchoredPosition = rect.anchoredPosition + new Vector2(textBounds, 0);
            cloneRect.localScale = Vector3.one; // I hate this
            
            // Update cloned text and set visibility
            cloneText.text = text.text;
            cloneText.gameObject.SetActive(enableScroll);
        }

        private void Update()
        {
            if (text.text != prevText) UpdateComponents();
            
            if (enableScroll)
            {
                offset += scrollSpeed * Time.deltaTime;
                offset %= textBounds;
            }
            else offset = 0;

            rect.anchoredPosition = new Vector2(startPos + offset, 0);
        }
    }
}