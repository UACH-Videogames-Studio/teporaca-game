using System.Collections.Generic;
using Stariluz;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stariluz
{
    public class ChopSceneManger : MonoBehaviour
    {
        public static ChopSceneManger Instance { get; private set; }

        [SerializeField] private string nextSceneName = "Narrative2"; // Cambia esto por el nombre real


        // Lista de estados de árboles
        private List<TreeChop> destroyedTrees = new List<TreeChop>();
        public SceneTransitionManager manager;

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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void TreeDestroyed(TreeChop tree)
        {
            // if (!destroyedTrees.Contains(tree))
            // {
            destroyedTrees.Add(tree);
            // }

            // if (!sceneLoading)
            // {
            //     sceneLoading = true;
            //     manager.LoadScene(nextSceneName);
            // }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Restaurar árboles destruidos si reiniciaste el capítulo
            foreach (var tree in destroyedTrees)
            {
                if (tree != null)
                {
                    tree.Restore();
                }
            }
            destroyedTrees.Clear();
        }
    }
}
