using UnityEngine;
namespace Stariluz
{
    public class TreeChop : MonoBehaviour
    {
        private int hitCount = 0;
        private Vector3 originalScale;
        private bool canBeHit = true;
        private float hitCooldown = 0.5f;
        private Coroutine currentCorroutine;

        void Start()
        {
            originalScale = transform.localScale;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Axe") && canBeHit)
            {
                HandleHit();
            }
        }

        void HandleHit()
        {
            TreeChopAudioManager.Instance?.PlayRandomChopSound(); // Añadido aquí

            hitCount++;

            float newScaleFactor = 1f;
            if (hitCount == 1)
            {
                newScaleFactor = 0.5f;
            }
            else if (hitCount == 2)
            {
                newScaleFactor = 0.25f;
            }
            else if (hitCount == 3)
            {
                NotifyTreeDestroyed();
                gameObject.SetActive(false);
                return;
            }
            else
            {
                return;
            }

            currentCorroutine = StartCoroutine(HitCooldown());

            Vector3 newScale = originalScale * newScaleFactor;
            float heightDifference = (originalScale.y - newScale.y) * transform.localScale.y / originalScale.y;

            // Ajustar la posición para mantener la base en el mismo lugar
            transform.position -= new Vector3(0, heightDifference / 2f, 0);
            transform.localScale = newScale;
        }

        System.Collections.IEnumerator HitCooldown()
        {
            canBeHit = false;
            yield return new WaitForSeconds(hitCooldown);
            canBeHit = true;
        }

        void NotifyTreeDestroyed()
        {
            ChopSceneManger.Instance.TreeDestroyed(this);
        }

        public void Restore()
        {
            canBeHit = true;
            hitCount = 0;
            transform.localScale = originalScale;
            gameObject.SetActive(true);
        }
    }
}
