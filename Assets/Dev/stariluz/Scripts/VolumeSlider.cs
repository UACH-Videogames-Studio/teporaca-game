using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Stariluz
{
    public class VolumeSlider : MonoBehaviour
    {
        public VolumeType volumeType;

        [Header("Slider UI")]
        public Image backgroundImage;
        public Image fillImage;

        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color selectedColor = Color.cyan;
        public Color normalColorBackground = Color.cyan;
        public Color selectedColorBackground = Color.cyan;

        private Slider slider;

        private void Start()
        {
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void Update()
        {
            // Verifica si este slider está actualmente seleccionado
            bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;

            // Cambia los colores dependiendo del estado
            if (backgroundImage != null)
                backgroundImage.color = isSelected ?  selectedColorBackground : normalColorBackground;

            // if (fillImage != null)
            //     fillImage.color = isSelected ? selectedColor : normalColor;
        }

        private void OnSliderValueChanged(float value)
        {
            AudioManager.Instance.SetVolume(volumeType, value);
        }
    }
}
