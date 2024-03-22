using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SaturnGame.UI
{
    public class UIAudioController : PersistentSingleton<UIAudioController>
    {
        [FormerlySerializedAs("ui_Back")] [SerializeField] private AudioClip uiBack;
        [FormerlySerializedAs("ui_Confirm")] [SerializeField] private AudioClip uiConfirm;
        [FormerlySerializedAs("ui_Impact")] [SerializeField] private AudioClip uiImpact;
        [FormerlySerializedAs("ui_StartGame")] [SerializeField] private AudioClip uiStartGame;
        [FormerlySerializedAs("ui_Navigate")] [SerializeField] private AudioClip uiNavigate;
        
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
                {
                    source.PlayOneShot(uiBack);
                    break;
                }

                case UISound.Confirm:
                {
                    source.PlayOneShot(uiConfirm);
                    break;
                }

                case UISound.Impact:
                {
                    source.PlayOneShot(uiImpact);
                    break;
                }

                case UISound.Navigate:
                {
                    source.PlayOneShot(uiNavigate);
                    break;
                }

                case UISound.StartGame:
                {
                    source.PlayOneShot(uiStartGame);
                    return;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(sound), sound, null);
                }
            }
        }
    }
}
