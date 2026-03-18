using UnityEngine;

namespace Exercise5
{
    [ExecuteInEditMode]
    public class FlipNormalMesh : MonoBehaviour
    {
        [SerializeField]
        private bool _isImmutableMesh = true;
        private MeshFilter _meshFilter = null;
        private Mesh _internalMesh = null;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnValidate()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }
            if (_meshFilter == null || _meshFilter.sharedMesh == null)
            {
                return;
            }
            Mesh mesh = _meshFilter.sharedMesh;
            if (mesh.name.EndsWith("_Flipped"))
            {
                return;
            }
            if (_isImmutableMesh)
            {
                _internalMesh = Instantiate(mesh);
                _internalMesh.name = mesh.name + "_Flipped";
                _meshFilter.sharedMesh = _internalMesh;
                mesh = _internalMesh;
            }
            else
            {
                mesh.name = mesh.name + "_Flipped";
            }
            FlipMesh(mesh);
        }
        void FlipMesh(Mesh mesh)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    int temp = triangles[j];
                    triangles[j] = triangles[j + 1];
                    triangles[j + 1] = temp;
                }
                mesh.SetTriangles(triangles, i);
            }
        }
    }
}