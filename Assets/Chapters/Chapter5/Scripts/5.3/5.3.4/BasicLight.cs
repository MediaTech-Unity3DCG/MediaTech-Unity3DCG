using UnityEngine;

namespace Chapter5.Sec5_3_4
{
    public enum BasicLightType { Point = 0, Directional = 1, Spot = 2 }
    public class BasicLight : MonoBehaviour
    {
        [SerializeField]
        public BasicLightType type = BasicLightType.Point;
        public Vector3 position { get { return transform.position; } }
        public Vector3 direction { get { return transform.forward; } }
        [SerializeField] public float intensity = 1.0f;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public float spotAngle = 30.0f;
        [SerializeField] public float range = 10.0f;
        [SerializeField] public Texture2D cookie = null;

    }
}
