using UnityEngine;

namespace Stariluz
{
    public class TreeChopAudioManager : MonoBehaviour
    {
        public static TreeChopAudioManager Instance;

        [SerializeField] private AudioClip[] chopSounds;
        [SerializeField] private AudioClip treeFellSound;
        [SerializeField] private AudioSource audioSource;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        public void PlayRandomChopSound()
        {
            if (chopSounds.Length == 0) return;

            int index = Random.Range(0, chopSounds.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = Random.Range(0.8f, 1.0f);
            audioSource.PlayOneShot(chopSounds[index]);
        }
        
        public void PlayTreeFellSound()
        {
            if (treeFellSound != null)
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.volume = Random.Range(0.8f, 1.0f);
                audioSource.PlayOneShot(treeFellSound);
        }
    }
}
