using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SaturnGame.Loading;
using System.Diagnostics;
using System;
using System.Threading.Tasks;

public class TextureTest : MonoBehaviour
{
    [SerializeField] private string path = "SongPacks/DONOTSHIP/IMG_Rebuff.png";
    [SerializeField] private RawImage img;
    [SerializeField] private Texture2D unknownJacket;
    void Update()
    {
        img.rectTransform.eulerAngles += new Vector3 (0, 0, 90) * Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.J)) LoadImage();
        if (Input.GetKeyDown(KeyCode.K)) LoadImageAsync();
    }


    void LoadImage()
    {
        //Stopwatch time = Stopwatch.StartNew();
        
        img.texture = ImageLoader.LoadJacket(Path.Combine(Application.streamingAssetsPath, path));

        //time.Stop();
        //long ts = time.ElapsedMilliseconds;
        //UnityEngine.Debug.Log($"{ts}ms");
    }

    async void LoadImageAsync()
    {
        
        if (img.texture)
        {
            Destroy(img.texture);
        }

        img.texture = await ImageLoader.LoadJacketWebRequest(Path.Combine(Application.streamingAssetsPath, path));
        if (img.texture == null)
            img.texture = unknownJacket;
    }
}
