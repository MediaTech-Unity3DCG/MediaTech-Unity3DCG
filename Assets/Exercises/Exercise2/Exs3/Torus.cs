using System.Collections.Generic;
using UnityEngine;

namespace Exercise2
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Torus : MonoBehaviour
    {
        [SerializeField, Range(8, 64)]
        protected int coreSegments = 16;
        [SerializeField, Range(8, 64)]
        protected int sectSegments = 8;
        [SerializeField, Range(2f, 10f)]
        protected float coreRad = 3f;
        [SerializeField, Range(0.1f, 4f)]
        protected float sectRad = 1f;

        [SerializeField]
        Material mat;

        private Mesh Build()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (var i = 0; i < coreSegments; i++)
            {
                var phi = 2f * Mathf.PI * i / (coreSegments - 1);
                for (var j = 0; j < sectSegments; j++)
                {
                    var theta = 2f * Mathf.PI * j / (sectSegments - 1);
                    vertices.Add(new Vector3(
                        (coreRad + sectRad * Mathf.Cos(theta)) * Mathf.Cos(phi),
                        (coreRad + sectRad * Mathf.Cos(theta)) * Mathf.Sin(phi),
                        sectRad * Mathf.Sin(theta)
                    ));
                }
            }

            for (var i = 0; i < coreSegments - 1; i++)
            {
                for (var j = 0; j < sectSegments - 1; j++)
                {
                    var a = i * sectSegments + j;
                    var b = a + 1;
                    var c = a + 1 + sectSegments;
                    var d = a + sectSegments;

                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(a); triangles.Add(d); triangles.Add(c);
                }
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        void Update()
        {
            this.GetComponent<MeshRenderer>().material = mat;
            this.GetComponent<MeshFilter>().mesh = Build();
        }
    }
}