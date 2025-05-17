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
        public AudioSource AudioSource => _audioSource;
        private Coroutine narrativeCoroutine;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        void Start()
        {
            _audioSource.clip = narrativeClip;
            _audioSource.playOnAwake = false;
            
            if (narrativeClip != null)
            {
                narrativeCoroutine = StartCoroutine(PlayNarrativeAtGameTime(startTimeInSeconds));
            }
            else
            {
                Debug.LogWarning("NarrativeManager: No se ha asignado ningún AudioClip.");
            }
        }

        IEnumerator PlayNarrativeAtGameTime(float gameTime)
        {
            // Espera hasta que el tiempo del juego alcance el tiempo deseado
            while (Time.time < gameTime)
            {
                // Si el juego está pausado, espera sin avanzar
                while (Time.timeScale == 0f)
                    yield return null;

                yield return null;
            }

            _audioSource.Play();
        }
    }
}
