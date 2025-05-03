using UnityEngine;

public class TreeChop : MonoBehaviour
{
    private int hitCount = 0;
    private Vector3 originalScale;
    private bool canBeHit = true;
    private float hitCooldown = 0.25f;


    void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(("Triger", other, other.CompareTag("Axe"), canBeHit));
        if (other.CompareTag("Axe") && canBeHit)
        {
            StartCoroutine(HitCooldown());
            HandleHit();
        }
    }

    void HandleHit()
    {
        Debug.Log(("Hit", hitCount));
        hitCount++;

        if (hitCount == 1)
        {
            // Disminuye el tamaño al 50%
            transform.localScale = originalScale * 0.5f;
        }
        else if (hitCount == 2)
        {
            // Disminuye un 25% más (de su tamaño original)
            transform.localScale = originalScale * 0.25f;
        }
        else if (hitCount >= 3)
        {
            // Desaparece (puedes usar Destroy o desactivarlo)
            NotifyTreeDestroyed();
            Destroy(gameObject);
        }
    }

    System.Collections.IEnumerator HitCooldown()
    {
        canBeHit = false;
        yield return new WaitForSeconds(hitCooldown);
        canBeHit = true;
    }
    public ChopSceneManger manager;
    void NotifyTreeDestroyed()
    {
        manager.TreeDestroyed(); // Llama al manejador
    }

}
