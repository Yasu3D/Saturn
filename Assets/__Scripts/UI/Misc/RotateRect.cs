using UnityEngine;

namespace SaturnGame.UI
{
public class RotateRect : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector3 speed;
    private Vector3 startRotation;

    private void Awake()
    {
        startRotation = rect.eulerAngles;
    }

    private void OnDisable()
    {
        rect.eulerAngles = startRotation;
    }

    private void Update()
    {
        rect.eulerAngles += speed * Time.deltaTime;
    }
}
}