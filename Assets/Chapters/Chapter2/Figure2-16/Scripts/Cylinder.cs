using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_16
{
    public class Cylinder : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Update()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            var radius = 1.0f;
            var height = 10.0f;
            var seg = 8;

            var bottom = -height / 2;
            var top = height / 2;

            for (var i = 0; i < seg; i++)
            {
                var theta = 2 * Mathf.PI * i / seg;
                var x = radius * Mathf.Cos(theta);
                var y = radius * Mathf.Sin(theta);
                vertices.Add(new Vector3(x, y, bottom));    // Vertex of the bottom face
                vertices.Add(new Vector3(x, y, top));       // Vertex of the top face
            }
            vertices.Add(new Vector3(0, 0, bottom));    // Center of the bottom face
            vertices.Add(new Vector3(0, 0, top)); 	    // Center of the top face


            for (var i = 0; i < seg; i++)
            {
                triangles.Add(seg * 2);                 // Bottom face
                triangles.Add(((i + 1) % seg) * 2);
                triangles.Add(i * 2);
                triangles.Add(seg * 2 + 1);             // Top Face
                triangles.Add(i * 2 + 1); 
                triangles.Add(((i + 1) % seg) * 2 + 1);
            }
            for (var i = 0; i < seg; i++)
            {
                triangles.Add(i * 2);                   // Upper face
                triangles.Add(((i + 1) % seg) * 2 + 1);
                triangles.Add(i * 2 + 1);
                triangles.Add(i * 2);                   // Lowwer face
                triangles.Add(((i + 1) % seg) * 2); 
                triangles.Add(((i + 1) % seg) * 2 + 1);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateNormals();  // Recalculate normal vectors
            mesh.RecalculateBounds();   // Recalculate bounding box

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }
}