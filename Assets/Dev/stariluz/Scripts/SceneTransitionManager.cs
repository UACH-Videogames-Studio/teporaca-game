using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool debugMode = false;


    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        if(!debugMode){
            StartCoroutine(Transition(sceneName));
        }else{
            Debug.LogWarning("Transition prevented. Debug mode is active.");
        }
    }

    private IEnumerator Transition(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        SetFadeAlpha(0f);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            SetFadeAlpha(a);
            yield return null;
        }
        SetFadeAlpha(1f);
    }

    private IEnumerator FadeIn()
    {
        SetFadeAlpha(1f);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeDuration);
            SetFadeAlpha(a);
            yield return null;
        }
        SetFadeAlpha(0f);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
}
