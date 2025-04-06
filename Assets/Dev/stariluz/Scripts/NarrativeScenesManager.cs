using UnityEngine;

public class NarrativeScenesManager : MonoBehaviour
{
    public static NarrativeScenesManager Instance { get; private set; }
    private bool sceneLoading = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (!sceneLoading)
        {
            sceneLoading = true;

            // Llama a tu SceneTransitionManager
            SceneTransitionManager.Instance.LoadScene(sceneName);
        }
    }
}
