using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_9
{
    public class Plane : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Update()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();

            var width = 10.0f;
            var height = 10.0f;
            var segW = 4;
            var segH = 5;

            for (var i = 0; i < segW + 1; i++)
            {
                for (var j = 0; j < segH + 1; j++)
                {
                    vertices.Add(new Vector3(
                        -width / 2 + width / segW * i,
                        -height / 2 + height / segH * j,
                        0
                    ));
                }
            }

            for (var i = 0; i < segW + 1; i++)
            {
                for (var j = 0; j < segH + 1; j++)
                {
                    uv.Add(new Vector2(
                        (float)i / segW,
                        (float)j / segH
                    ));
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

                    triangles.Add(a);   // Lower left
                    triangles.Add(b);   // Upper left
                    triangles.Add(c);   // Upper right

                    triangles.Add(a);   // Lower left
                    triangles.Add(c);   // Upper right
                    triangles.Add(d);   // Lower right
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }
}