using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.UI
{
    public class RotateRect : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Vector3 speed;
        private Vector3 startRotation;

        void Awake()
        {
            startRotation = rect.eulerAngles;
        }

        void OnDisable()
        {
            rect.eulerAngles = startRotation;
        }

        void Update()
        {
            rect.eulerAngles += speed * Time.deltaTime;
        }
    }
}