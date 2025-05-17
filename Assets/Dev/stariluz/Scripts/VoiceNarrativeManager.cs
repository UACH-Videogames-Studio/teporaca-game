using System.Collections;
using UnityEngine;

namespace Stariluz
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceNarrativeManager : MonoBehaviour
    {
        [Header("Narrative Settings")]
        public AudioClip narrativeClip;         // El audio de la narrativa
        public float startTimeInSeconds = 10f;  // Cuándo empezar la reproducción (en tiempo del juego)

        private AudioSource _audioSource;
        public AudioSource AudioSource=>_audioSource;

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = narrativeClip;
            _audioSource.playOnAwake = false;
            _audioSource.PlayScheduled(10);

        }
    }
}
