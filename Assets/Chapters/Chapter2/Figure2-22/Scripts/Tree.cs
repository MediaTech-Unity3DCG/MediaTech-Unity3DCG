using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_22
{
    public class Tree : MonoBehaviour
    {
        void Branch(int g, Turtle T)
        {
            if (g > 0)
            {
                T.Shorten();                // S	
                var T1 = T.Duplicate();     // [
                T1.TurnLeft();              // L
                T1.DrawLine();              // F
                Branch(g - 1, T1);          // A
                T.TurnRight();              // ]R
                T.DrawLine();               // F
                Branch(g - 1, T);			// A
            }
        }

        void Update()
        {
            var T = new Turtle();
            T.DrawLine();
            Branch(8, T);
        }
    }

    public class Turtle
    {
        Vector3 pos;            // Current position
        float ang;              // Direction of movement (degree)
        float len;              // Distance of movement
        
        public Turtle()
        {
            pos = Vector3.zero;
            ang = 90;
            len = 1;
        }

        public void Shorten()
        {
            len *= 0.7f;
        }
        public void TurnLeft()
        {
            ang += 30;
        }
        public void TurnRight()
        {
            ang -= 30;
        }
        public void DrawLine()
        {
            var phi = Mathf.Deg2Rad * ang;
            var dir = new Vector3(Mathf.Cos(phi), Mathf.Sin(phi), 0);
            var end = pos + dir * len;

            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var seg = 8;
            var rad = 0.02f;
            var ey = Vector3.forward;
            var ex = Vector3.Cross(ey, dir);
            for (var i = 0; i < seg; i++)
            {
                var theta = 2 * Mathf.PI / seg * i;
                vertices.Add(pos + ex * Mathf.Cos(theta) * rad 
                    + ey * Mathf.Sin(theta) * rad);
                vertices.Add(end + ex * Mathf.Cos(theta) * rad 
                    + ey * Mathf.Sin(theta) * rad);
            }
            for (var i = 0; i < seg; i++)
            {
                triangles.Add(i * 2);
                triangles.Add(((i + 1) % seg) * 2 + 1);
                triangles.Add(i * 2 + 1);
                triangles.Add(i * 2);
                triangles.Add(((i + 1) % seg) * 2);
                triangles.Add(((i + 1) % seg) * 2 + 1);
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var material = new Material(Shader.Find("Standard"));
            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
            
            pos = end;
        }
        public Turtle Duplicate()
        {
            var t = new Turtle();
            t.pos = pos;
            t.ang = ang;
            t.len = len;
            return t;
        }
    }
}