using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void PlayStereo()
    {
        audioSource.panStereo = 0;
        audioSource.Play();
    }

    public void PlayLeft()
    {
        audioSource.panStereo = -1;
        audioSource.Play();
    }

    public void PlayRight()
    {
        audioSource.panStereo = 1;
        audioSource.Play();
    }
}
