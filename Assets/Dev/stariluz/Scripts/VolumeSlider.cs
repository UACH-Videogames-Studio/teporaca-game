// En un script llamado VolumeSlider.cs
using UnityEngine;
using UnityEngine.UI;

namespace Stariluz
{
    public class VolumeSlider : MonoBehaviour
    {
        public VolumeType volumeType;

        private Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            AudioManager.Instance.SetVolume(volumeType, value);
        }
    }
}
