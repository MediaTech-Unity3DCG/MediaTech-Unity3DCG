using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise3
{
    //cylinderおよび側面にプロジェクションカメラを生成する
    //cylinderの粗さと大きさを設定    
    public class MakeCylinderCamera : MonoBehaviour
    {
        [SerializeField, Range(1, 100)] private int hight = 1; //cylinderの高さ
        [SerializeField, Range(1, 20)] private int radius = 1; //cylinderの半径
        [SerializeField, Range(3, 128)] private int segments = 8; //cylinderの側面数

        [SerializeField] private GameObject cameras; //カメラのローカル座標用
        [SerializeField] private GameObject objWorld; //撮影場所の座標
        [SerializeField] private GameObject cylinders; //オブジェクトを生成するパス
        [SerializeField] private Shader _shader;

        void Awake()
        {
            double wide = 2 * radius * Mathf.Tan(180.0f / (float)segments * Mathf.Deg2Rad);
            var myCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            myCylinder.transform.parent = cylinders.transform;
            myCylinder.transform.position = Vector3.zero;
            myCylinder.transform.localScale
                = new Vector3((float)radius * 1.9999f, (float)hight / 2.0f, (float)radius * 1.9999f
                ); //上下の面のためにCylinder生成

            for (int i = 0; i < segments; i++)
            {
                //側面数だけCylinderの側面とそれに対応するプロジェクションカメラを生成
                //MakeCamera
                var CylCam = new GameObject("CylCam" + i, typeof(Camera));
                CylCam.transform.position = cameras.transform.position;
                CylCam.transform.parent = this.transform;

                CylCam.transform.Rotate(0.0f, 360.0f / (float)segments * (float)i, 0.0f);
                CylCam.gameObject.AddComponent<EditProjectionMat4>();


                //MakeRenderTexture
                var CylTex = new RenderTexture(256, 256, 16);

                var cam = CylCam.GetComponent<Camera>();
                cam.targetTexture = CylTex;

                var CylMat = new Material(_shader);
                CylMat.mainTexture = CylTex;

                //MakeFrontPlane
                var CylPlaneF = GameObject.CreatePrimitive(PrimitiveType.Plane);
                CylPlaneF.transform.parent = cylinders.transform;
                CylPlaneF.gameObject.name = "CylPlaneF" + i;

                CylPlaneF.transform.Rotate(90.0f, 180.0f, 0.0f);
                CylPlaneF.transform.position = new Vector3(0f, 0f, -radius);
                CylPlaneF.transform.localScale = new Vector3(0.1f * (float)wide, 0.1f, 0.1f * hight);
                CylPlaneF.transform.RotateAround(Vector3.zero, Vector3.up, 360.0f / segments * i);

                CylPlaneF.gameObject.GetComponent<Renderer>().material = CylMat;

                //SetValue
                var EditProMat = CylCam.GetComponent<EditProjectionMat4>();
                EditProMat.camMover = cameras;
                EditProMat.planePos = new Vector3(CylPlaneF.transform.position.x + objWorld.transform.position.x,
                    CylPlaneF.transform.position.y + objWorld.transform.position.y,
                    CylPlaneF.transform.position.z + objWorld.transform.position.z
                );
                EditProMat.planeHight = (float)hight;
                EditProMat.planeWide = (float)wide;
            }
        }
    }
}
