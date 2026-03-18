using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter3
{
    //カメラ行列への理解を深める教育用
    public class EditToCameraMat : MonoBehaviour
    {
        void Awake()
        {
            Camera cam = GetComponent<Camera>();


            Matrix4x4 mat = new Matrix4x4(
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.worldToCameraMatrix = mat;
        }
    }
}
