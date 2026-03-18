using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exercise3
{
    // メインカメラの動きに合わせてオブジェクトに鏡の動きをさせる
    //xy平面対称だけ対応
    public class MirrorCamMover : MonoBehaviour
    {
        [SerializeField] private GameObject mainCam;

        // Start is called before the first frame update
        void Start()
        {
            // メインカメラのローカル位置をコピー
            this.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y,
                -mainCam.transform.position.z
            );
        }

        // Update is called once per frame
        void Update()
        {
            // メインカメラのローカル位置をコピー
            this.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y,
                -mainCam.transform.position.z
            );
        }
    }
}
