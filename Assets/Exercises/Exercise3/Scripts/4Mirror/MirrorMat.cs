using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise3
{
    //メインカメラから見たときに鏡に見えるようなテクスチャを作る鏡カメラ用
    //このコードを適用した鏡カメラを平面にRender Textureとして設定することで使用    
    public class MirrorMat : MonoBehaviour
    {
        [SerializeField] private GameObject camMover;

        private Camera cam;

        private float m00;
        private float m11;
        private float m22;
        private float m23;

        private Vector2 shift;

        void Awake()
        {
            cam = GetComponent<Camera>();

            shift = new Vector2(0.0f, 0.0f);

            //ProjectionMatrixの各要素を更新
            shift.x = camMover.transform.position.x / 2.5f;
            shift.y = -camMover.transform.position.y / 2.5f;

            m00 = -camMover.transform.position.z / 2.5f;
            m11 = -camMover.transform.position.z / 2.5f;
            m22 = (camMover.transform.position.z - 100) / (camMover.transform.position.z + 100);
            m23 = (camMover.transform.position.z * 200) / (camMover.transform.position.z + 100);

            Matrix4x4 mat = new Matrix4x4(
                new Vector4(m00, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, m11, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, m22, m23),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f)
            ).transpose; //わかりやすいように転置行列で記述

            cam.projectionMatrix = mat;
        }

        // Update is called once per frame
        void Update()
        {
            //ProjectionMatrixの各要素を更新
            shift.x = camMover.transform.position.x / 2.5f;
            shift.y = -camMover.transform.position.y / 2.5f;

            m00 = -camMover.transform.position.z / 2.5f;
            m11 = -camMover.transform.position.z / 2.5f;
            m22 = (camMover.transform.position.z - 100) / (camMover.transform.position.z + 100);
            m23 = (camMover.transform.position.z * 200) / (camMover.transform.position.z + 100);

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
