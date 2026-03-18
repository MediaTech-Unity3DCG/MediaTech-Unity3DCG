using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_1Alt
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Quad : MonoBehaviour
    {
        [SerializeField]
        Material material;

        void Start()
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

            this.GetComponent<MeshFilter>().mesh = mesh;
            this.GetComponent<MeshRenderer>().material = material;
        }
    }
}