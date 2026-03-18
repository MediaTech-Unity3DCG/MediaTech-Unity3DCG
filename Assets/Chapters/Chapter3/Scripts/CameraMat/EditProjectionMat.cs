using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter3
{
    //投影行列への理解を深める教育用
    [ExecuteInEditMode]
    public class EditProjectionMat : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 360.0f)]
        //回転用の変数
        private float myRotation;

        private Camera cam;

        private float sin;
        private float cos;

        private float m00;
        private float m11;
        private float m22;
        private float m23;

        void Awake()
        {
            cam = GetComponent<Camera>();

            //回転確認用
            //cos = Mathf.Cos(Mathf.Deg2Rad * myRotation);
            //sin = Mathf.Sin(Mathf.Deg2Rad * myRotation);

            m00 = Mathf.Sqrt(3.0f) * 9.0f / 8.0f;
            m11 = Mathf.Sqrt(3.0f);
            m22 = -101.0f / 99.0f;
            m23 = -200.0f / 99.0f;


            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f)).transpose;//わかりやすいように転置行列で記述

            //回転確認用
            /*
            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00 * cos, 0.0f, -m00 * sin, 0.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(m22 * sin, 0.0f, m22 * cos, m23),
                new Vector4(-sin, 0.0f, -cos, 0.0f)).transpose;//わかりやすいように転置行列で記述
            */

            cam.projectionMatrix = mat;
        }
        //回転確認用
        /*
            void Update()
            {
                cos = Mathf.Cos(Mathf.Deg2Rad * myRotation);
                sin = Mathf.Sin(Mathf.Deg2Rad * myRotation);

                Matrix4x4 mat = new Matrix4x4(
                    new Vector4(m00 * cos, 0.0f, -m00 * sin, 0.0f),
                    new Vector4(0.0f, m11, 0.0f, 0.0f),
                    new Vector4(m22 * sin, 0.0f, m22 * cos, m23),
                    new Vector4(-sin, 0.0f, -cos, 0.0f)).transpose;//わかりやすいように転置行列で記述

                cam.projectionMatrix = mat;

                //myRotation += 0.1f;

            }
        */
    }
}
