using UnityEngine;

public class ChopSceneManger : MonoBehaviour
{
    public static ChopSceneManger Instance { get; private set; }

    [SerializeField] private string nextSceneName = "Narrative2"; // Cambia esto por el nombre real

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
    public SceneTransitionManager manager;

    public void TreeDestroyed()
    {
        if (!sceneLoading)
        {
            sceneLoading = true;

            // Llama a tu SceneTransitionManager
            manager.LoadScene(nextSceneName);
        }
    }
}
