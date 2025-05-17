using UnityEngine;
using UnityEngine.Video;

namespace Stariluz
{
    public class VideoEndSceneLoader : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public SceneTransitionManager sceneTransitionManager;
        public string nextScene;

        void Start()
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            videoPlayer.loopPointReached += OnVideoEnd;
        }

        void OnVideoEnd(VideoPlayer vp)
        {
            sceneTransitionManager.LoadScene(nextScene);
        }
    }
}
