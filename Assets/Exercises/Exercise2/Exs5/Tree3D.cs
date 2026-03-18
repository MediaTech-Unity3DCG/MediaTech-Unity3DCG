using System.Collections.Generic;
using UnityEngine;

namespace Exercise2.Tree3D
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Tree3D : MonoBehaviour
    {
        [SerializeField]
        Material material;
        void Branch(int g, Turtle t, MeshData md)
        {
            if (g > 0)
            {
                t.Shorten();                        // S	
                var t1 = t.Duplicate();       // [
                t1.Yawing();                        // Y
                t1.DrawLine(md);                    // F
                Branch(g - 1, t1, md);           // A
                var t2 = t.Duplicate();       // [
                t2.Rolling();                       // R
                t2.Yawing();                        // Y
                t2.DrawLine(md);                    // F
                Branch(g - 1, t2, md);           // A
                t.Rolling();                        // ]R
                t.Rolling();                        // R
                t.Yawing();                         // Y
                t.DrawLine(md);                     // F
                Branch(g - 1, t, md);            // A
            }
        }

        void Start()
        {
            var md = new MeshData();
            var turtle = new Turtle();
            turtle.DrawLine(md);
            Branch(8, turtle, md);
            
            this.GetComponent<MeshFilter>().mesh = md.Build();
            this.GetComponent<MeshRenderer>().material = material;
        }
    }

    public class Turtle
    {
        Vector3 pos;            // Current position
        Vector3 dir;            // Direction of movement
        Vector3 nor;            // Direction of upward
        float len;              // Distance of movement

        public Turtle()
        {
            pos = Vector3.zero;
            dir = Vector3.up;
            nor = Vector3.back;
            len = 1;
        }

        public void Shorten()
        {
            len *= 0.7f;
        }
        public void Rolling()
        {
            nor = Quaternion.AngleAxis(120, dir) * nor;
        }
        public void Yawing()
        {
            dir = Quaternion.AngleAxis(60, nor) * dir;
        }
        public void DrawLine(MeshData md)
        {
            var end = pos + dir * len;

            var seg = 8;
            var rad = 0.01f;
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
            t.dir = dir;
            t.nor = nor;
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
            // Use 32-bit indices to support meshes with more than 65,535 vertices
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}