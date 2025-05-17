using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
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
        public string voicesVolumeParam = "VoicesVolume";

        [Header("Sliders")]
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Slider voicesSlider;

        private string masterVolumePrefKey = "Volume_Master";
        private string musicVolumePrefKey = "Volume_Music";
        private string sfxVolumePrefKey = "Volume_SFX";
        private string voicesVolumePrefKey = "Volume_Voices";

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

        private void Start()
        {
            SetupStartVolume();
        }
        
        public void SetupStartVolume()
        {
            SetupSlider(masterSlider, PlayerPrefs.GetFloat(masterVolumePrefKey, 1f));
            SetupSlider(musicSlider, PlayerPrefs.GetFloat(musicVolumePrefKey, 1f));
            SetupSlider(sfxSlider, PlayerPrefs.GetFloat(sfxVolumePrefKey, 1f));
            SetupSlider(voicesSlider, PlayerPrefs.GetFloat(voicesVolumePrefKey, 1f));
            SetVolume(VolumeType.Master, PlayerPrefs.GetFloat(masterVolumePrefKey, 1f));
            SetVolume(VolumeType.Music, PlayerPrefs.GetFloat(musicVolumePrefKey, 1f));
            SetVolume(VolumeType.SFX, PlayerPrefs.GetFloat(sfxVolumePrefKey, 1f));
            SetVolume(VolumeType.Voices, PlayerPrefs.GetFloat(voicesVolumePrefKey, 1f));
        }

        public void SetVolume(VolumeType volumeType, float value)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    Instance.SetMasterVolume(value);
                    break;
                case VolumeType.Music:
                    Instance.SetMusicVolume(value);
                    break;
                case VolumeType.SFX:
                    Instance.SetSFXVolume(value);
                    break;
                case VolumeType.Voices:
                    Instance.SetVoicesVolume(value);
                    break;
            }
        }

        // volume va de 0.0001f a 1.0f
        public void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(masterVolumePrefKey, volume);
            SetVolume(masterVolumeParam, masterVolumePrefKey, volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolume(musicVolumeParam, musicVolumePrefKey, volume);
        }

        public void SetSFXVolume(float volume)
        {
            SetVolume(sfxVolumeParam, sfxVolumePrefKey, volume);
        }

        public void SetVoicesVolume(float volume)
        {
            SetVolume(sfxVolumeParam, sfxVolumePrefKey, volume);
        }

        private void SetVolume(string parameter, string volumePrefkey, float volume)
        {
            PlayerPrefs.SetFloat(volumePrefkey, volume);
            
            float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat(parameter, dB);
            Debug.Log((parameter, volume, dB));
        }

        private void SetupSlider(Slider slider, float value)
        {
            if (slider == null) return;
            slider.SetValueWithoutNotify(value);
        }
    }
}