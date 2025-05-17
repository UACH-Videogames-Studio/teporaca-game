using UnityEngine;

namespace Stariluz
{
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        public AudioClip confirmClip;
        public AudioClip continueClip;
        public AudioClip pauseClip;
        public AudioClip resumeClip;
        public AudioClip cancelClip;

        private AudioSource audioSource;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Usar el AudioSource adjunto
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("UIAudioManager necesita un componente AudioSource en el mismo GameObject.");
            }
        }

        public void PlayConfirmSound()
        {
            PlaySound(confirmClip);
        }

        public void PlayContinueSound()
        {
            PlaySound(continueClip);
        }

        public void PlayPauseSound()
        {
            PlaySound(pauseClip);
        }

        public void PlayResumeSound()
        {
            PlaySound(resumeClip);
        }

        public void PlayCancelSound()
        {
            PlaySound(cancelClip);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("UIAudioManager: No se asignó el AudioClip.");
            }
        }
    }
}
