using System.Collections.Generic;
using UnityEngine;

namespace Exercise2
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RegularTriangleMesh : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Update()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            var size = 1.0f;
            var segW = 4;
            var segH = 5;

            for (var i = 0; i < segW + 1; i++)
            {
                for (var j = 0; j < segH + 1; j++)
                {
                    var x = i * size;
                    var y = j * size *  Mathf.Sqrt(3) / 2;
                    if (j % 2 == 1) {
                        x += size * 0.5f;
                    }
                    vertices.Add(new Vector3(x, y, 0));
                }
            }

            for (var i = 0; i < segW; i++)
            {
                for (var j = 0; j < segH; j++)
                {
                    var a = i * (segH + 1) + j;
                    var b = a + 1;
                    var c = b + (segH + 1);
                    var d = c - 1;

                    if (j % 2 == 0) {
                        triangles.Add(a); triangles.Add(b); triangles.Add(d);
                        triangles.Add(b); triangles.Add(c); triangles.Add(d);
                    } else {
                        triangles.Add(a); triangles.Add(b); triangles.Add(c);
                        triangles.Add(a); triangles.Add(c); triangles.Add(d);
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }
}