using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Chapter3
{
    public class CameraMover : MonoBehaviour
    {
        // WASD：前後左右の移動
        // QE：上昇・降下
        // 右ドラッグ：カメラの回転
        // 左ドラッグ：前後左右の移動
        // スペース：カメラ操作の有効・無効の切り替え
        // P：回転を実行時の状態に初期化する

        //カメラの移動量
        //    [SerializeField, Range(0.1f, 20.0f)]
        private float _positionStep = 20.0f;

        //マウス感度
        //[SerializeField, Range(30.0f, 300.0f)]
        private float _mouseSensitive = 150.0f;

        // カメラ移動の速度
        [SerializeField, Range(0.1f, 10.0f)]
        private float speed = 2.0f;

        //カメラ操作の有効無効
        public static bool _cameraMoveActive = true;
        //カメラのtransform
        private Transform _camTransform;
        //マウスの始点
        private Vector3 _startMousePos;
        //カメラ回転の始点情報
        private Vector3 _presentCamRotation;
        private Vector3 _presentCamPos;
        //初期状態 Rotation
        private Quaternion _initialCamRotation;
        //UIメッセージの表示
        private bool _uiMessageActiv;

        private float scroll;

        void Start()
        {
            _camTransform = this.gameObject.transform;

            //初期回転の保存
            _initialCamRotation = this.gameObject.transform.rotation;
        }

        void Update()
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Mouse.current.position.value;

            CamControlIsActive(); //カメラ操作の有効無効

            if (_cameraMoveActive)
            {
                ResetCameraRotation(); //回転角度のみリセット
                CameraRotationMouseControl(); //カメラの回転 マウス
                CameraSlideMouseControl(); //カメラの縦横移動 マウス
                                           //CameraPositionKeyControl(); //カメラのローカル移動 キー
                CameraDolly();              //ホイールでのドリーイン・アウト
            }
        }

        //カメラ操作の有効無効
        public void CamControlIsActive()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _cameraMoveActive = !_cameraMoveActive;

                if (_uiMessageActiv == false)
                {
                    StartCoroutine(DisplayUiMessage());
                }
                //Debug.Log("CamControl : " + _cameraMoveActive);
            }
        }

        //回転を初期状態にする
        private void ResetCameraRotation()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                this.gameObject.transform.rotation = _initialCamRotation;
                //Debug.Log("Cam Rotate : " + _initialCamRotation.ToString());
            }
        }

        //カメラの回転 マウス
        private void CameraRotationMouseControl()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            Vector3 mousePos = mouse.position.value;
            bool altPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            bool shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

            if (mouse.leftButton.wasPressedThisFrame && altPressed)
            {
                _startMousePos = mousePos;
                _presentCamRotation.x = _camTransform.transform.eulerAngles.x;
                _presentCamRotation.y = _camTransform.transform.eulerAngles.y;

                _presentCamPos = _camTransform.position;
            }
            else if (mouse.leftButton.wasPressedThisFrame)
            {
                _startMousePos = mousePos;
                _presentCamPos = _camTransform.position;
            }

            if (mouse.leftButton.isPressed && altPressed)
            {
                float x = (_startMousePos.x - mousePos.x) / Screen.width;
                float y = (_startMousePos.y - mousePos.y) / Screen.height;

                //回転開始角度 ＋ マウスの変化量 * マウス感度
                float eulerX = _presentCamRotation.x + y * _mouseSensitive;
                float eulerY = _presentCamRotation.y + x * _mouseSensitive;

                //_camTransform.position = new Vector3(_presentCamPos.x * Mathf.Cos(eulerX) + _presentCamPos.z * Mathf.Sin(eulerX),
                //                                     _presentCamPos.y,
                //                                   -_presentCamPos.x * Mathf.Sin(eulerX) + _presentCamPos.z * Mathf.Sin(eulerX)
                //                                     );
                _camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
            }
            else if (mouse.leftButton.isPressed && shiftPressed)
            {
                //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
                float x = (_startMousePos.x - mousePos.x) / Screen.width;
                float y = (_startMousePos.y - mousePos.y) / Screen.height;

                x = x * _positionStep;
                y = y * _positionStep;

                Vector3 velocity = _camTransform.rotation * new Vector3(x, y, 0);
                velocity = velocity + _presentCamPos;
                _camTransform.position = velocity;
            }
        }

        //カメラの移動 マウス
        private void CameraSlideMouseControl()
        {
            var mouse = Mouse.current;
            Vector3 mousePos = mouse.position.value;

            if (mouse.middleButton.wasPressedThisFrame)
            {
                _startMousePos = mousePos;
                _presentCamPos = _camTransform.position;
            }

            if (mouse.middleButton.isPressed)
            {
                //(移動開始座標 - マウスの現在座標) / 解像度 で正規化
                float x = (_startMousePos.x - mousePos.x) / Screen.width;
                float y = (_startMousePos.y - mousePos.y) / Screen.height;

                x = x * _positionStep;
                y = y * _positionStep;

                Vector3 velocity = _camTransform.rotation * new Vector3(x, y, 0);
                velocity = velocity + _presentCamPos;
                _camTransform.position = velocity;
            }
        }

        //カメラのローカル移動 キー
        private void CameraPositionKeyControl()
        {
            var keyboard = Keyboard.current;
            Vector3 campos = _camTransform.position;

            if (keyboard.dKey.isPressed) { campos += _camTransform.right * Time.deltaTime * _positionStep; }
            if (keyboard.aKey.isPressed) { campos -= _camTransform.right * Time.deltaTime * _positionStep; }
            if (keyboard.eKey.isPressed) { campos += _camTransform.up * Time.deltaTime * _positionStep; }
            if (keyboard.qKey.isPressed) { campos -= _camTransform.up * Time.deltaTime * _positionStep; }
            if (keyboard.wKey.isPressed) { campos += _camTransform.forward * Time.deltaTime * _positionStep; }
            if (keyboard.sKey.isPressed) { campos -= _camTransform.forward * Time.deltaTime * _positionStep; }

            _camTransform.position = campos;
        }

        private void CameraDolly()
        {
            // マウスホイールの回転値を変数 scroll に渡す
            scroll = Mouse.current.scroll.y.ReadValue() / 120f;

            // カメラの前後移動処理
            //（カメラが向いている方向 forward に変数 scroll と speed を乗算して加算する）
            this.gameObject.transform.position += transform.forward * scroll * speed;
        }

        //UIメッセージの表示
        private IEnumerator DisplayUiMessage()
        {
            _uiMessageActiv = true;
            float time = 0;
            while (time < 2)
            {
                time = time + Time.deltaTime;
                yield return null;
            }
            _uiMessageActiv = false;
        }

        /*void OnGUI()
        {
            if (_uiMessageActiv == false) { return; }
            GUI.color = Color.black;
            if (_cameraMoveActive == true)
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 有効");
            }

            if (_cameraMoveActive == false)
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 30, 100, 20), "カメラ操作 無効");
            }
        }*/

    }
}
