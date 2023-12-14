using System.Collections;
using System.Collections.Generic;
using SaturnGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class ViewRectController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image mask;

    [SerializeField] private Vector2 rBounds = new(0, 0.3237f);
    [SerializeField] private Vector2 oBounds = new(0, 840);

    public void SetViewRect(int position, int scale)
    {
        float currentAspect = (float)Screen.width / Screen.height;

        float p = (position - 50) * 0.01f; // remap from [0<>100] to [-0.5<>+0.5]
        float s = scale * 0.01f;

        float r = Mathf.Lerp(rBounds.x, rBounds.y, s);
        float o = Mathf.LerpUnclamped(oBounds.x, oBounds.y, p);

        float x, y, w, h;
        if (currentAspect < 1.0f)
        {
            // Portrait
            Debug.Log("Portrait");
            x = 0.5f * (1 - s);
            y = (1 - currentAspect) * 0.5f + p * (1 - currentAspect);
            w = s;
            h = currentAspect * s;

            y += h * ((0.5f / s) - 0.5f);
            mask.rectTransform.localPosition = new(0, o, 0);
        }
        else
        {
            // Landscape
            Debug.Log("Landscape");
            x = (1 - (1 / currentAspect)) * 0.5f + p * (1 - (1 / currentAspect));
            y = 0.5f * (1 - s);
            w = 1 / currentAspect * s;
            h = s;

            x += w * ((0.5f / s) - 0.5f);
            mask.rectTransform.localPosition = new(o, 0, 0);
        }

        mainCamera.rect = new(x,y,w,h);
        mask.material.SetFloat("_Radius", r);
    }
}
