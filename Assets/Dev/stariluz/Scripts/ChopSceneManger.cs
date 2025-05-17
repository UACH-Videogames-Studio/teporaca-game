using System.Collections.Generic;
using Stariluz;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stariluz
{
    public class ChopSceneManger : MonoBehaviour
    {
        public static ChopSceneManger Instance { get; private set; }

        // Lista de estados de árboles
        private List<TreeChop> destroyedTrees = new List<TreeChop>();
        public int destroyedTreesThreshold=5;

        private void Awake()
        {
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
            destroyedTrees.Add(tree);

            if(destroyedTrees.Count>=destroyedTreesThreshold){
                GameManager.Instance.LoadNextScene();
            }
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
