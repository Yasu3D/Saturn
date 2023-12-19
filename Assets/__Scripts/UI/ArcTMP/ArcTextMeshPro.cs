//MIT License

//Copyright(c) 2019 Antony Vitillo(a.k.a. "Skarredghost")

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

// Simplified version of CurvedTextMeshPro, which can be found here:
// https://github.com/TonyViT/CurvedTextMeshPro

using UnityEngine;
using TMPro;

namespace SaturnGame.UI
{
    [ExecuteInEditMode]
    public class ArcTextMeshPro : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float letterSpacing;
        [SerializeField] private float radius;
        [SerializeField] private float angleOffset;

        private float prevLetterSpacing;
        private float prevRadius;
        private float prevAngleOffset;

        private bool ParametersChanged()
        {
            bool value = prevLetterSpacing != letterSpacing || prevRadius != radius || prevAngleOffset != angleOffset;

            prevLetterSpacing = letterSpacing;
            prevRadius = radius;
            prevAngleOffset = angleOffset;
            return value;
        }

        void Awake()
        {
            if (textComponent == null)
                textComponent = gameObject.GetComponent<TMP_Text>();

            UpdateText();
        }

        void Update()
        {
            if (!ParametersChanged() && !textComponent.havePropertiesChanged) return;

            UpdateText();
        }

        public void UpdateText()
        {
            Vector3[] vertices;
            Matrix4x4 matrix;

            textComponent.ForceMeshUpdate();
            
            TMP_TextInfo textInfo = textComponent.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0) return;

            //float boundsMin = textComponent.rectTransform.rect.xMin;
            //float boundsMax = textComponent.rectTransform.rect.xMax;
            float boundsMin = textComponent.bounds.min.x;
            float boundsMax = textComponent.bounds.max.x;

            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;



                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 charMidBaselinePos = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
                //textInfo.characterInfo[i].origin
                float zeroToOnePos = Mathf.InverseLerp(boundsMin, boundsMax, charMidBaselinePos.x);

                vertices[vertexIndex + 0] += -charMidBaselinePos;
                vertices[vertexIndex + 1] += -charMidBaselinePos;
                vertices[vertexIndex + 2] += -charMidBaselinePos;
                vertices[vertexIndex + 3] += -charMidBaselinePos;

                matrix = GetMatrix(zeroToOnePos, textInfo, i);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
            }

            textComponent.UpdateVertexData();
        }

        private Matrix4x4 GetMatrix(float zeroToOnePos, TMP_TextInfo textInfo, int charIndex)
        {
            float scaledLetterSpacing = letterSpacing * textComponent.rectTransform.rect.width * 0.15f;
            float angle = ((zeroToOnePos * scaledLetterSpacing) + angleOffset) * Mathf.Deg2Rad;

            float x0 = Mathf.Cos(angle);
            float y0 = Mathf.Sin(angle);

            float lineRadius = radius - textInfo.lineInfo[0].lineExtents.max.y * textInfo.characterInfo[charIndex].lineNumber;

            Vector2 newMidBaselinePos = new(x0 * lineRadius, -y0 * lineRadius);

            return Matrix4x4.TRS(newMidBaselinePos, Quaternion.AngleAxis(-Mathf.Atan2(y0, x0) * Mathf.Rad2Deg - 90, Vector3.forward), Vector3.one);
        }
    }
}
