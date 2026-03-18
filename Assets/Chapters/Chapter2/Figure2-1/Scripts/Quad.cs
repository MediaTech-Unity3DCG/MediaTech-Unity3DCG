using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_1
{
    public class Quad : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Update()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            vertices.Add(new Vector3(-5, -5, 0));
            vertices.Add(new Vector3(-5, 5, 0));
            vertices.Add(new Vector3(5, 5, 0));
            vertices.Add(new Vector3(5, -5, 0));

            triangles.Add(0);   // (-5, -5, 0): Lower left
            triangles.Add(1);   // (-5,  5, 0): Upper left
            triangles.Add(2);   // ( 5,  5, 0): Upper right

            triangles.Add(0);   // (-5, -5, 0): Lower left
            triangles.Add(2);   // ( 5,  5, 0): Upper right
            triangles.Add(3);   // ( 5, -5, 0): Lower right

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }
}