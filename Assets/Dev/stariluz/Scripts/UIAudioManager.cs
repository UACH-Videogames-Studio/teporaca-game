using UnityEngine;

namespace Stariluz
{
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [Header("Audio Clips")]
        public AudioClip successClip;
        public AudioClip nextClip;
        public AudioClip pauseClip;
        public AudioClip resumeClip;
        public AudioClip leaveClip;
        public AudioClip restartClip;

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

        public void PlaySuccessSound()
        {
            PlaySound(successClip);
        }

        public void PlayNextSound()
        {
            PlaySound(nextClip);
        }

        public void PlayPauseSound()
        {
            PlaySound(pauseClip);
        }

        public void PlayResumeSound()
        {
            PlaySound(resumeClip);
        }

        public void PlayLeaveSound()
        {
            PlaySound(leaveClip);
        }

        public void PlayRestartSound()
        {
            PlaySound(restartClip);
        }
        public void PlaySound(AudioClip clip)
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
