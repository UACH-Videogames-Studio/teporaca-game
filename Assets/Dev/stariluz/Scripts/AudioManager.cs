using UnityEngine;
using UnityEngine.Audio;
namespace Stariluz
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Mixer")]
        public AudioMixer audioMixer;

        [Header("Exposed Parameters")]
        public string masterVolumeParam = "MasterVolume";
        public string musicVolumeParam = "MusicVolume";
        public string sfxVolumeParam = "SFXVolume";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Opcional
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // volume va de 0.0001f a 1.0f
        public void SetMasterVolume(float volume)
        {
            SetVolume(masterVolumeParam, volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolume(musicVolumeParam, volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetVolume(sfxVolumeParam, volume);
        }

        public void SetVolume(VolumeType volumeType, float value)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    AudioManager.Instance.SetMasterVolume(value);
                    break;
                case VolumeType.Music:
                    AudioManager.Instance.SetMusicVolume(value);
                    break;
                case VolumeType.SFX:
                    AudioManager.Instance.SetSFXVolume(value);
                    break;
            }
        }

        private void SetVolume(string parameter, float volume)
        {
            // Convierte el valor lineal a decibelios: 0.0001 -> -80 dB, 1.0 -> 0 dB
            float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat(parameter, dB);
        }
    }
}