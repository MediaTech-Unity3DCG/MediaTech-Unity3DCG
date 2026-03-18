using UnityEngine;
using UnityEngine.InputSystem;

namespace Chapter3
{
    // メインカメラの動きに合わせてオブジェクトに同じ動きをさせる
    // 動作確認用
    // Rで一時停止
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameObject mainCam; //メインカメラ

        private bool drowFlag; //一時停止用のフラグ

        // Start is called before the first frame update
        void Start()
        {
            drowFlag = true;
            //メインカメラのローカル位置をコピー
            this.transform.localPosition = mainCam.transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            //一時停止の処理
            if (Keyboard.current.rKey.wasPressedThisFrame) drowFlag = !drowFlag;

            if (drowFlag)
            {
                //メインカメラのローカル位置をコピー
                this.transform.localPosition = mainCam.transform.position;
            }
        }
    }
}
