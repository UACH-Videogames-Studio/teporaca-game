using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Stariluz
{
    /// <summary>
    /// Controls scroll animation on self. It can be inside a Narrative Manager that activates the panel.
    /// At start it usually is needed deactivated.
    /// <br/>
    /// <br/>
    /// <author>Luz E. Adora @stariluz</author>
    /// </summary>
    public class NarrativeScroll : MonoBehaviour
    {

        [Tooltip("Speed at wich the scroll is moving")]
        public float scrollSpeed = 50f;

        [Tooltip("Delay after the animation")]
        [SerializeField] private float endDelay = 1f;

        [Tooltip("Run the animation since the start")]
        [SerializeField] private bool runOnStart = true;


        [Tooltip("Canvas Rect Transform")]
        [SerializeField] private RectTransform visibleArea; // Asigna esto en el inspector al viewport o al Canvas rect


        private RectTransform rectTransform;
        private float yStartPosition;
        private delegate void UpdateFunction();
        private UpdateFunction updateFunction;

        [SerializeField]
        UnityEvent onEndEvent;

        void Start()
        {
            if (runOnStart)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }

            rectTransform = GetComponent<RectTransform>();
            yStartPosition = rectTransform.anchoredPosition.y;
        }
        void OnDisable()
        {
            StopAllCoroutines();
        }

        public void Activate()
        {
            // Debug.Log("Dev @stariluz - Activate narrative scroll");
            updateFunction = OnMoveUpdate;
        }
        public void Deactivate()
        {
            // Debug.Log("Dev @stariluz - Deactivate narrative scroll");
            updateFunction = InactiveUpdate;
        }

        void Update()
        {
            updateFunction();
        }

        void InactiveUpdate() { }

        void OnMoveUpdate()
        {
            rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

            if (IsOutOfScreen())
            {
                updateFunction = InactiveUpdate;
                EndTransitionAfterDelay(endDelay);
            }
        }

        protected bool IsOutOfScreen()
        {

            float scrollY = rectTransform.anchoredPosition.y;
            float contentHeight = rectTransform.rect.height;
            float visibleHeight = visibleArea.rect.height;

            Debug.Log((scrollY, yStartPosition,contentHeight, visibleHeight));
            
            return scrollY >= yStartPosition + contentHeight + visibleHeight;
        }

        private void EndTransitionAfterDelay(float delay)
        {
            StartCoroutine(EndTransitionCoroutine(delay));
        }

        private IEnumerator EndTransitionCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            onEndEvent?.Invoke();
            Debug.Log("DEV @stariuz - After delay event invoked");

            gameObject.SetActive(false);
        }
    }
}