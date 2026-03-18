using UnityEngine;

namespace Chapter2.Figure2_21
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
        Vector3 pos;
        float ang;
        float len;
        
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
            Debug.DrawLine(pos, end);
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