using System.Collections;
using UnityEngine;

namespace Stariluz
{
    /// <summary>
    /// Canvas containing the narrative panel that will be scrolled down.
    /// This allows to control the panel start event.
    /// <br/>
    /// <br/>
    /// <author>Luz E. Adora @stariluz</author>
    /// </summary>
    public class NarrativeManager : MonoBehaviour
    {
        [Tooltip("Panel we want to execute the animation")]
        [SerializeField] private GameObject canvas;

        [Tooltip("Delay previous to the animation")]
        [SerializeField] private float startDelay = 0f;

        private void Start()
        {
            ShowCreditsAfterDelay(startDelay);
        }

        public void ShowCreditsAfterDelay(float delay)
        {
            StartCoroutine(ShowCreditsCoroutine(delay));
        }
        private IEnumerator ShowCreditsCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay); // Espera delay segundos
            ShowPanel(); // Muestra los créditos después de esperar
        }

        private void ShowPanel()
        {
            canvas.SetActive(true);
        }
    }
}
