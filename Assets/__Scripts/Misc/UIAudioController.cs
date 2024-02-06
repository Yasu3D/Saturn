using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaturnGame.UI
{
    public class UIAudioController : PersistentSingleton<UIAudioController>
    {
        [SerializeField] private AudioClip ui_Back;
        [SerializeField] private AudioClip ui_Confirm;
        [SerializeField] private AudioClip ui_Impact;
        [SerializeField] private AudioClip ui_StartGame;
        [SerializeField] private AudioClip ui_Navigate;
        
        [SerializeField] private AudioSource source;

        public enum UISound
        {
            
            Back,
            Confirm,
            Impact,
            Navigate,
            StartGame,
        }

        public void PlaySound(UISound sound)
        {
            switch (sound)
            {
                case UISound.Back:
                    source.PlayOneShot(ui_Back);
                    break;

                case UISound.Confirm:
                    source.PlayOneShot(ui_Confirm);
                    break;

                case UISound.Impact:
                    source.PlayOneShot(ui_Impact);
                    break;

                case UISound.Navigate:
                    source.PlayOneShot(ui_Navigate);
                    break;

                case UISound.StartGame:
                    source.PlayOneShot(ui_StartGame);
                    return;
            }
        }
    }
}
