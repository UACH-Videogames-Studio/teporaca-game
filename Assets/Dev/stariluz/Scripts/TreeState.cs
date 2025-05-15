using UnityEngine;

namespace Stariluz
{
    [System.Serializable]
    public class TreeState
    {
        public Vector3 position;
        public Vector3 scale;
        public bool destroyed;

        public TreeState(Vector3 position, Vector3 scale, bool destroyed)
        {
            this.position = position;
            this.scale = scale;
            this.destroyed = destroyed;
        }
    }
}
