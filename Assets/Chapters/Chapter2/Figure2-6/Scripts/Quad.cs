using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_6
{
    public class Quad : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Update()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();

            vertices.Add(new Vector3(-5, -5, 0));
            vertices.Add(new Vector3(-5, 5, 0));
            vertices.Add(new Vector3(5, 5, 0));
            vertices.Add(new Vector3(5, -5, 0));

            uv.Add(new Vector2(0, 0));   // Lower left
            uv.Add(new Vector2(0, 1));   // Upper left
            uv.Add(new Vector2(1, 1));   // Upper right
            uv.Add(new Vector2(1, 0));   // Lower right

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);

            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(3);

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }
}