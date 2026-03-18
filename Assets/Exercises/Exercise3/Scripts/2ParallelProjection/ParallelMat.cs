using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise3 {
    //カメラを並行投影に変更する
    public class ParallelMat : MonoBehaviour
    {
        private Camera cam;

        private float m00;
        private float m11;
        private float m22;
        private float m23;

        void Awake()
        {
            cam = GetComponent<Camera>();
          
            m00 = 1.0f / 2.4f;
            m11 = 1.0f / 2.7f;
            m22 = -2.0f / 99.0f; 
            m23 = -101.0f / 99.0f;
            
            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f)).transpose;//わかりやすいように転置行列で記述     

            cam.projectionMatrix = mat;
        }
    }
}
