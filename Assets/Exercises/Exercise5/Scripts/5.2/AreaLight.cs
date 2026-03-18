using UnityEngine;

namespace Exercise5
{
    [ExecuteAlways]
    public class AreaLight : MonoBehaviour
    {
        [SerializeField] public float intensity = 1.0f;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public Texture2D texture = null;
        [SerializeField] public Mesh mesh = null;
        [SerializeField] private Material _material = null;
        private MeshRenderer _meshRenderer = null;
        private MeshFilter _meshFilter = null;
        private void OnValidate()
        {
            if (!_material) { return; }
            _material.SetColor("_Color", new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1f));
            _material.SetTexture("_MainTex", texture);
            if (_meshRenderer == null)
            {
                _meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (_meshRenderer == null)
                {
                    _meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }
            _meshRenderer.sharedMaterial = _material;
            if (_meshFilter == null)
            {
                _meshFilter = gameObject.GetComponent<MeshFilter>();
                if (_meshFilter == null)
                {
                    _meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }
            _meshFilter.sharedMesh = mesh;
        }
    }
}