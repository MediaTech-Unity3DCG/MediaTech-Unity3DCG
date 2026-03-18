using System.Collections.Generic;
using UnityEngine;

namespace Exercise2
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Flatten : MonoBehaviour
    {
        [SerializeField]
        Material mat;

        private void Start()
        {
            this.GetComponent<MeshFilter>().mesh = Build();
            this.GetComponent<MeshRenderer>().material = mat;
        }

        private Mesh Build()
        {
            var mesh = this.GetComponent<MeshFilter>().mesh;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (var i = 0; i < mesh.triangles.Length / 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    vertices.Add(mesh.vertices[mesh.triangles[i * 3 + j]]);
                    triangles.Add(i * 3 + j);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}