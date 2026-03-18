using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Exercise5
{
    // カスケードごとの計算結果を保持
    public struct CascadeData
    {
        public float Near;
        public float Far;
        public Vector3[] LocalVertices;
        public Vector3[] WorldVertices;
        public Vector3 SphereCenter;
        public float SphereRadius;
        public float Bias; // 個別バイアス用
    }

    public class CascadeLight : MonoBehaviour
    {
        [SerializeField] public bool visualizeCascades = true;
        [Header("Light Settings")]
        [SerializeField] public float intensity = 1.0f;
        [SerializeField] public Color color = Color.white;
        public Vector3 direction => transform.forward;

        [Header("Shadow Settings")]
        [SerializeField] public int  shadowResolution = 1024;
        [SerializeField] public float shadowNearPlane = 0.1f;
        [SerializeField] private Shader depthShader; // インスペクタで指定可能に
        [SerializeField] private Camera _mainCamera = null;

        [Header("Cascade Splits & Biases")]
        // 分割率とバイアスをセットで管理しやすく配列に
        [SerializeField, Range(1, 4)] private int _cascadeCount = 4;
        [SerializeField, Range(0, 1)] private float[] _cascadeSplits = { 0.05f, 0.1f, 0.2f, 0.65f };
        [SerializeField] private float[] _cascadeBiases = { 0.001f, 0.002f, 0.005f, 0.01f };


        public List<float> cascadeSplits => _cascadeSplits.ToList();
        public List<float> cascadeBiases => _cascadeBiases.ToList();

        public Matrix4x4 GetShadowVPMatrix(int index)
        {
            if (index < 0 || index >= _cascadeCameras.Count) return Matrix4x4.identity;

            Camera cam = _cascadeCameras[index];
            // View行列とProjection行列を掛け合わせる
            Matrix4x4 V = cam.worldToCameraMatrix;
            Matrix4x4 P = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
            return P * V;
        }

        // キャッシュ・管理用
        private readonly List<CascadeData> _cascadeDataCache = new List<CascadeData>();
        private readonly List<Camera> _cascadeCameras = new List<Camera>();
        private readonly List<RenderTexture> _cascadeShadowMaps = new List<RenderTexture>();

        private Matrix4x4 _lastCameraMatrix;
        private float _lastFov, _lastAspect;

        #region Lifecycle
        private void OnEnable()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            InitializeResources();
        }

        private void OnDisable() => CleanupResources();

        private void Update()
        {
            if (_mainCamera == null) return;

            // 1. カメラの変化を検知してカスケード計算を更新
            if (HasCameraChanged())
            {
                UpdateCascadeData();
                UpdateCameraStates();
            }

            // 2. シャドウマップ（RenderTexture）の整合性チェック
            ValidateShadowMaps();
        }
        #endregion

        #region Initialization & Cleanup
        private void InitializeResources()
        {
            if (depthShader == null) depthShader = Shader.Find("Unlit/Exercise5/BasicLightDepth");
            CreateCascadeCameras();
            RebuildShadowMaps();
        }

        private void CleanupResources()
        {
            foreach (var cam in _cascadeCameras)
                if (cam) { if (Application.isPlaying) Destroy(cam.gameObject); else DestroyImmediate(cam.gameObject); }
            _cascadeCameras.Clear();

            foreach (var rt in _cascadeShadowMaps)
                if (rt) rt.Release();
            _cascadeShadowMaps.Clear();
        }

        private void CreateCascadeCameras()
        {
            for (int i = 0; i < _cascadeSplits.Length; i++)
            {
                var go = new GameObject($"[Cascade_Cam_{i}]") { hideFlags = HideFlags.DontSave };
                var cam = go.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.Depth;
                cam.enabled = true; // 基本はUpdateで制御
                _cascadeCameras.Add(cam);
            }
        }
        #endregion

        #region Core Logic
        private bool HasCameraChanged()
        {
            bool changed = _mainCamera.transform.localToWorldMatrix != _lastCameraMatrix ||
                           !Mathf.Approximately(_mainCamera.fieldOfView, _lastFov) ||
                           !Mathf.Approximately(_mainCamera.aspect, _lastAspect);

            if (changed)
            {
                _lastCameraMatrix = _mainCamera.transform.localToWorldMatrix;
                _lastFov = _mainCamera.fieldOfView;
                _lastAspect = _mainCamera.aspect;
            }
            return changed;
        }

        private void UpdateCascadeData()
        {
            _cascadeDataCache.Clear();
            float nearClip    = _mainCamera.nearClipPlane;
            float farClip     = _mainCamera.farClipPlane;
            float currentNear = nearClip;

            for (int i = 0; i < _cascadeSplits.Length; i++)
            {
                float currentFar = currentNear + (farClip - nearClip) * _cascadeSplits[i];

                var (localCenter, radius) = CalculateBoundingSphereLocal(currentNear, currentFar);

                var data = new CascadeData
                {
                    Near = currentNear,
                    Far = currentFar,
                    LocalVertices = GetFrustumVerticesLocal(_mainCamera, currentFar, currentNear),
                    SphereRadius = radius,
                    SphereCenter = _mainCamera.transform.TransformPoint(localCenter),
                    Bias = (i < _cascadeBiases.Length) ? _cascadeBiases[i] : _cascadeBiases.LastOrDefault()
                };

                data.WorldVertices = data.LocalVertices.Select(v => _mainCamera.transform.TransformPoint(v)).ToArray();

                _cascadeDataCache.Add(data);
                currentNear = currentFar;
            }
        }

        private void UpdateCameraStates()
        {
            if (_cascadeCameras.Count != _cascadeDataCache.Count) return;

            for (int i = 0; i < _cascadeCameras.Count; i++)
            {
                var data = _cascadeDataCache[i];
                var cam = _cascadeCameras[i];

                // 1. 回転を同期
                cam.transform.rotation = transform.rotation;

                // 2. テクセルスナッピング
                float shadowResolution = _cascadeShadowMaps[i].width;
                float worldTexelSize = (data.SphereRadius * 2.0f) / shadowResolution;

                // ライト空間（カメラのローカル）での中心位置を求める
                Vector3 lightSpaceCenter = cam.transform.InverseTransformPoint(data.SphereCenter);

                // XYのみスナップ
                lightSpaceCenter.x = Mathf.Floor(lightSpaceCenter.x / worldTexelSize) * worldTexelSize;
                lightSpaceCenter.y = Mathf.Floor(lightSpaceCenter.y / worldTexelSize) * worldTexelSize;

                // 3. スナップした座標をワールド空間に戻す
                Vector3 snappedSphereCenter = cam.transform.TransformPoint(lightSpaceCenter);

                // --- 4. 光源のローカルZ基準でのカメラ配置 ---

                // snappedSphereCenterの「現在のワールド空間での深度」を、このスクリプト（光源）の向きで評価
                float currentCenterWorldZ = Vector3.Dot(transform.forward, snappedSphereCenter);
                // 光源の位置（transform.position）を基準とした、このカスケード中心の相対的な深さ
                float lightBaseWorldZ = Vector3.Dot(transform.forward, transform.position);

                // 光源位置から _shadowNearZ 分だけ進んだところが「描画開始面（Near Plane）」
                float absoluteNearPlaneWorldZ = lightBaseWorldZ + shadowNearPlane;

                // 共通の Near Plane 面からカスケード中心までの距離
                float distanceToCenter = currentCenterWorldZ - absoluteNearPlaneWorldZ;

                // カメラを描画開始面（absoluteNearPlaneWorldZ）に配置
                cam.transform.position = snappedSphereCenter - transform.forward * distanceToCenter;

                // 5. パラメータ設定
                cam.orthographic = true;
                cam.orthographicSize = data.SphereRadius;
                cam.aspect = 1.0f;
                cam.nearClipPlane = 0.0f;

                // FarClipは「共通の描画開始面」から「各カスケード球の後端」までをカバー
                cam.farClipPlane = distanceToCenter + data.SphereRadius;

                cam.depth = _mainCamera.depth - 1 - i;

                if (depthShader) cam.SetReplacementShader(depthShader, "RenderType");
                cam.targetTexture = _cascadeShadowMaps[i];
            }
        }
        #endregion

        #region Shadow Map Management
        private void ValidateShadowMaps()
        {
            if (_cascadeShadowMaps.Count != _cascadeSplits.Length ||
                (_cascadeShadowMaps.Count > 0 && _cascadeShadowMaps[0].width != shadowResolution))
            {
                RebuildShadowMaps();
            }
        }

        private void RebuildShadowMaps()
        {
            foreach (var rt in _cascadeShadowMaps) if (rt) rt.Release();
            _cascadeShadowMaps.Clear();

            for (int i = 0; i < _cascadeSplits.Length; i++)
            {
                var rt = new RenderTexture(shadowResolution, shadowResolution, 32, RenderTextureFormat.Shadowmap)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                rt.Create();
                _cascadeShadowMaps.Add(rt);
            }
        }
        #endregion

        #region Geometry Helpers
        private Vector3[] GetFrustumVerticesLocal(Camera cam, float max, float min)
        {
            float tan = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float hN = min * tan, wN = hN * cam.aspect;
            float hF = max * tan, wF = hF * cam.aspect;

            return new Vector3[] {
                new Vector3(wN, hN, min), new Vector3(wN, -hN, min), new Vector3(-wN, -hN, min), new Vector3(-wN, hN, min),
                new Vector3(wF, hF, max), new Vector3(wF, -hF, max), new Vector3(-wF, -hF, max), new Vector3(-wF, hF, max)
            };
        }

        private (Vector3 center, float radius) CalculateBoundingSphereLocal(float n, float f)
        {
            float tan = Mathf.Tan(_mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float k = tan * tan * (1f + _mainCamera.aspect * _mainCamera.aspect);
            float z = (n + f) * 0.5f * (1f + k);
            float r = Mathf.Sqrt(Mathf.Pow(f - z, 2) + Mathf.Pow(f * tan, 2) + Mathf.Pow(f * tan * _mainCamera.aspect, 2));
            return (new Vector3(0, 0, z), r);
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
        {
            if (_cascadeDataCache.Count == 0) UpdateCascadeData();

            for (int i = 0; i < _cascadeDataCache.Count; i++)
            {
                var data = _cascadeDataCache[i];
                Color color = Color.HSVToRGB((float)i / _cascadeDataCache.Count, 1.0f, 1.0f);

                Gizmos.color = color;
                DrawFrustum(data.WorldVertices);

                Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
                Gizmos.DrawWireSphere(data.SphereCenter, data.SphereRadius);
            }
        }

        private void DrawFrustum(Vector3[] v)
        {
            for (int j = 0; j < 4; j++)
            {
                Gizmos.DrawLine(v[j], v[(j + 1) % 4]);
                Gizmos.DrawLine(v[j + 4], v[((j + 1) % 4) + 4]);
                Gizmos.DrawLine(v[j], v[j + 4]);
            }
        }
        #endregion
        private void Awake()
        {
            // 実行時に未設定なら取得
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) Debug.LogError("Main Cameraが見つかりません。");
            }
        }

        private void Reset()
        {
            // コンポーネントを初めてアタッチした時に実行
            if (_mainCamera == null) _mainCamera = Camera.main;
        }
        private void OnValidate()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            // 1. 配列のサイズをカウントに合わせる
            AdjustArraySize(ref _cascadeSplits, _cascadeCount, 1.0f / _cascadeCount);
            AdjustArraySize(ref _cascadeBiases, _cascadeCount, 0.001f);

            // 2. Splitの合計を1に正規化する（利便性のため）
            NormalizeSplits();
        }

        private void AdjustArraySize(ref float[] array, int targetSize, float defaultValue)
        {
            if (array == null)
            {
                array = new float[targetSize];
                for (int i = 0; i < targetSize; i++) array[i] = defaultValue;
                return;
            }

            if (array.Length != targetSize)
            {
                System.Array.Resize(ref array, targetSize);
            }
        }

        private void NormalizeSplits()
        {
            if (_cascadeSplits == null || _cascadeSplits.Length == 0) return;

            float sum = _cascadeSplits.Sum();
            if (sum <= 0) // 全て0などの不正な入力対策
            {
                for (int i = 0; i < _cascadeSplits.Length; i++) _cascadeSplits[i] = 1.0f / _cascadeSplits.Length;
            }
            else
            {
                for (int i = 0; i < _cascadeSplits.Length; i++) _cascadeSplits[i] /= sum;
            }
        }

        public RenderTexture GetShadowMap(int index) => (index >= 0 && index < _cascadeShadowMaps.Count) ? _cascadeShadowMaps[index] : null;
    }
}