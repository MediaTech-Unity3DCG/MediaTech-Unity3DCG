using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chapter3
{
    //メインカメラから見たときに立体的に見えるようなテクスチャを作るプロジェクションカメラ用
    //このコードを適用したプロジェクションカメラを平面にRender Textureとして設定することで使用    
    //各要素の計算は本参照
    //投影面の位置大きさは固定(5*5, 中心(0,0,0)の前提)
    public class EditProjectionMat2 : MonoBehaviour
    {
        [SerializeField] private GameObject camMover;

        private Camera cam;

        //行列の要素
        private float m00;
        private float m11;
        private float m22;
        private float m23;

        //near plane, far planeの位置
        private float near_p;
        private float far_p;

        private Vector2 shift;

        void Awake()
        {
            cam = GetComponent<Camera>();

            shift = new Vector2(0.0f, 0.0f);

            //ProjectionMatrixの各要素を更新
            shift.x = -camMover.transform.localPosition.x / 2.5f;
            shift.y = -camMover.transform.localPosition.y / 2.5f;

            m00 = -camMover.transform.localPosition.z / 2.5f;
            m11 = -camMover.transform.localPosition.z / 2.5f;

            //ビュー座標系はカメラが-z方向を向いているので負
            near_p = -1.0f;
            far_p = -100.0f;

            m22 = (near_p + far_p) / (near_p - far_p);
            m23 = (-2.0f * near_p * far_p) / (near_p - far_p);

            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.projectionMatrix = mat;
        }

        void Update()
        {
            //ProjectionMatrixの各要素を更新
            shift.x = -camMover.transform.localPosition.x / 2.5f;
            shift.y = -camMover.transform.localPosition.y / 2.5f;

            m00 = -camMover.transform.localPosition.z / 2.5f;
            m11 = -camMover.transform.localPosition.z / 2.5f;

            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, shift.x, 0.0f),
                new Vector4(0.0f, m11, shift.y, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, -1.0f, 0.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.projectionMatrix = mat;

        }
    }
}
