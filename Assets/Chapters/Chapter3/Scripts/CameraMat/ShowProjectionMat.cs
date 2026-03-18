using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter3
{
    //投影行列をDebugLogに表示するスクリプト
    public class ShowProjectionMat : MonoBehaviour
    {
        void Start()
        {
            //mat:投影行列
            Camera cam = GetComponent<Camera>();
            Matrix4x4 mat = cam.projectionMatrix;

            //Matrix表示
            Debug.Log(this.name + " ProjectionMat" + 
                "\n" + mat.m00.ToString("f4") + "  " + mat.m01.ToString("f4") + "  " + mat.m02.ToString("f4") + "  " + mat.m03.ToString("f4") +
                "\n" + mat.m10.ToString("f4") + "  " + mat.m11.ToString("f4") + "  " + mat.m12.ToString("f4") + "  " + mat.m13.ToString("f4") +
                "\n" + mat.m20.ToString("f4") + "  " + mat.m21.ToString("f4") + "  " + mat.m22.ToString("f4") + "  " + mat.m23.ToString("f4") +
                "\n" + mat.m30.ToString("f4") + "  " + mat.m31.ToString("f4") + "  " + mat.m32.ToString("f4") + "  " + mat.m33.ToString("f4"));
        }
    }
}
