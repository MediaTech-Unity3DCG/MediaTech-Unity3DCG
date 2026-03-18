using System.Collections.Generic;
using UnityEngine;

namespace Chapter2.Figure2_22Alt
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Tree : MonoBehaviour
    {
        [SerializeField]
        Material material;
        void Branch(int g, Turtle t, MeshData md)
        {
            if (g > 0)
            {
                t.Shorten();                    // S	
                var t1 = t.Duplicate();   // [
                t1.TurnLeft();                  // L
                t1.DrawLine(md);                // F
                Branch(g - 1, t1, md);       // A
                t.TurnRight();                  // ]R
                t.DrawLine(md);                 // F
                Branch(g - 1, t, md);	    // A
            }
        }

        void Start()
        {
            var md = new MeshData();
            var T = new Turtle();
            T.DrawLine(md);
            Branch(8, T, md);
            
            this.GetComponent<MeshFilter>().mesh = md.Build();
            this.GetComponent<MeshRenderer>().material = material;
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
        public void DrawLine(MeshData md)
        {
            var phi = Mathf.Deg2Rad * ang;
            var dir = new Vector3(Mathf.Cos(phi), Mathf.Sin(phi), 0);
            var end = pos + dir * len;

            var seg = 8;
            var rad = 0.02f;
            var ey = Vector3.forward;
            var ex = Vector3.Cross(ey, dir);
            var margin = md.VertCount();
            for (var i = 0; i < seg; i++)
            {
                var theta = 2 * Mathf.PI / seg * i;
                md.vertices.Add(pos + ex * Mathf.Cos(theta) * rad
                    + ey * Mathf.Sin(theta) * rad);
                md.vertices.Add(end + ex * Mathf.Cos(theta) * rad
                    + ey * Mathf.Sin(theta) * rad);
            }
            for (var i = 0; i < seg; i++)
            {
                md.triangles.Add(margin + i * 2);
                md.triangles.Add(margin + ((i + 1) % seg) * 2 + 1);
                md.triangles.Add(margin + i * 2 + 1);
                md.triangles.Add(margin + i * 2);
                md.triangles.Add(margin + ((i + 1) % seg) * 2);
                md.triangles.Add(margin + ((i + 1) % seg) * 2 + 1);
            }

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

    public class MeshData{
        public List<Vector3> vertices;
        public List<int> triangles;
        public MeshData()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
        }
        public int VertCount()
        {
            return vertices.Count;
        }
        public Mesh Build()
        {
            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}