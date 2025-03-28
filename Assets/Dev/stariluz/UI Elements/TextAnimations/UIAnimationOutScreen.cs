using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CreditsOpenAnimation : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] Vector2 direction;

    private RectTransform rTransform;
    private float startY, startX = 0;
    private delegate void UpdateFunction();
    private UpdateFunction updateFunction;

    [SerializeField]
    UnityEvent endEvent;
    protected int directionX = 0;
    protected int directionY = 0;
    protected float limitX = 0;
    protected float limitY = 0;
    void Start()
    {
        rTransform = GetComponent<RectTransform>();
        startX = rTransform.anchoredPosition.x;
        startY = rTransform.anchoredPosition.y;
        updateFunction = EmptyFunction;

        directionX = (int)Mathf.Sign(direction.x);
        directionY = (int)Mathf.Sign(direction.y);
        limitX = startX + directionX * (rTransform.rect.width + 100f);
        limitY = startX + directionY * (rTransform.rect.height + 200f);

    }
    void OnDisable(){
        StopAllCoroutines();
    }

    void Update()
    {
        updateFunction();
    }
    void EmptyFunction() { }
    void MoveFunction()
    {
        // Desplazar en la direccion
        rTransform.anchoredPosition += direction * speed * Time.deltaTime;

        // Verificar si esta fuera de la pantalla
        if (
            directionX == 1 && rTransform.anchoredPosition.x >= limitX
            ||
            directionX == -1 && rTransform.anchoredPosition.x <= limitX
            ||
            directionY == 1 && rTransform.anchoredPosition.y >= limitY
            ||
            directionY == -1 && rTransform.anchoredPosition.y <= limitY
        )
        {
            updateFunction = EmptyFunction;
            StartCoroutine(AfterEndDelay());
        }
    }
    public void Activate()
    {
        updateFunction = MoveFunction;
    }
    private IEnumerator AfterEndDelay()
    {
        gameObject.SetActive(false); // Ocultar el objeto fuera de pantalla
        yield return new WaitForSeconds(1f);
        endEvent?.Invoke();
    }
}
