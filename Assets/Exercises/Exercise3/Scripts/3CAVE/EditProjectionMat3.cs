using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise3
{
    //メインカメラから見たときに立体的に見えるようなテクスチャを作るプロジェクションカメラ用
    //このコードを適用したプロジェクションカメラを平面にRender Textureとして設定することで使用    
    //各要素の計算はスライド参照
    //投影面の位置と大きさから自動で計算パラメータを計算
    //【注意】例外処理をしていないので，投影面とメインカメラが重なるとエラーを吐きます
    public class EditProjectionMat3 : MonoBehaviour
    {
        public GameObject camMover;
        //投影面の情報
        public Vector3 planePos;
        public float planeHight;
        public float planeWide;

        private Camera cam;

        private float m00;
        private float m11;
        private float m02;
        private float m12;
        private float m22;
        private float m23;

        private Vector3 shift;

        void Start()
        {
            cam = GetComponent<Camera>();

            //ProjectionMatrixの各要素を更新
            shift = new Vector3
            {
                x = (cam.worldToCameraMatrix.m00 * planePos.x) + (cam.worldToCameraMatrix.m01 * planePos.y)
                    + (cam.worldToCameraMatrix.m02 * planePos.z) + cam.worldToCameraMatrix.m03,
                y = (cam.worldToCameraMatrix.m10 * planePos.x) + (cam.worldToCameraMatrix.m11 * planePos.y)
                    + (cam.worldToCameraMatrix.m12 * planePos.z) + cam.worldToCameraMatrix.m13,
                z = (cam.worldToCameraMatrix.m20 * planePos.x) + (cam.worldToCameraMatrix.m21 * planePos.y)
                    + (cam.worldToCameraMatrix.m22 * planePos.z) + cam.worldToCameraMatrix.m23
            };

            m00 = -shift.z * 2.0f / planeWide;
            m11 = -shift.z * 2.0f / planeHight;
            m02 = shift.x * 2.0f / planeWide;
            m12 = shift.y * 2.0f / planeHight;
            m22 = -101.0f / 99.0f;
            m23 = -200.0f / 99.0f;

            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, m02, 0.0f),
                new Vector4(0.0f, m11, m12, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.projectionMatrix = mat;
        }

        void Update()
        {

            //ProjectionMatrixの各要素を更新
            shift.x = (cam.worldToCameraMatrix.m00 * planePos.x) + (cam.worldToCameraMatrix.m01 * planePos.y)
                + (cam.worldToCameraMatrix.m02 * planePos.z) + cam.worldToCameraMatrix.m03;
            shift.y = (cam.worldToCameraMatrix.m10 * planePos.x) + (cam.worldToCameraMatrix.m11 * planePos.y)
                + (cam.worldToCameraMatrix.m12 * planePos.z) + cam.worldToCameraMatrix.m13;
            shift.z = (cam.worldToCameraMatrix.m20 * planePos.x) + (cam.worldToCameraMatrix.m21 * planePos.y)
                + (cam.worldToCameraMatrix.m22 * planePos.z) + cam.worldToCameraMatrix.m23;

            m00 = -shift.z * 2.0f / planeWide;
            m11 = -shift.z * 2.0f / planeHight;
            m02 = shift.x * 2.0f / planeWide;
            m12 = shift.y * 2.0f / planeHight;

            Matrix4x4 mat = new Matrix4x4(

                new Vector4(m00, 0.0f, m02, 0.0f),
                new Vector4(0.0f, m11, m12, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.projectionMatrix = mat;

        }
    }
}
