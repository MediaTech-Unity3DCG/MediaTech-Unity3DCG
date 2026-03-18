using UnityEngine;
using UnityEngine.Rendering;

namespace Exercise5
{
    public enum ShadowType { None, Default, PCF, VSM }
    public class SoftLight : MonoBehaviour
    {
        [SerializeField]
        public BasicLightType type = BasicLightType.Point;
        [SerializeField] public float intensity = 1.0f;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public float spotAngle = 30.0f;
        [SerializeField] public float range = 10.0f;
        [SerializeField] public Texture2D cookie = null;
        [SerializeField] public ShadowType shadowType = ShadowType.None;
        [SerializeField] public int shadowResolution = 1024;
        [SerializeField, Range(0, 1)] public float shadowBias = 0.005f;
        [SerializeField] public float shadowNearPlane = 0.1f;
        [SerializeField] public float shadowFarPlane = 1000.0f;
        [SerializeField] private Material _shadowBlurMaterial = null;
        private Shader _depthShaderDefault = null;
        private Shader _depthShaderVSM = null;
        private Shader depthShaderDefault
        {
            get
            {
                if (_depthShaderDefault == null)
                {
                    _depthShaderDefault = Shader.Find("Unlit/Exercise5/BasicLightDepth");
                }
                return _depthShaderDefault;
            }
        }
        private Shader depthShaderVSM
        {
            get
            {
                if (_depthShaderVSM == null)
                {
                    _depthShaderVSM = Shader.Find("Unlit/Exercise5/VarianceLightDepth");
                }
                return _depthShaderVSM;
            }
        }

        class VSMData
        {
            private RenderTexture _depthRT;
            private RenderTexture _blurRT;
            private CommandBuffer _blurCmd;
            private Material _blurMaterial;
            private int _resolution;

            public VSMData(Material blurMaterial, int resolution)
            {
                _depthRT = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.RGFloat);
                _depthRT.Create();
                _blurRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RGFloat);
                _blurRT.Create();
                _blurMaterial = blurMaterial;

                _blurCmd = new CommandBuffer();
                _blurCmd.name = "VSM Blur Pass";
                int tempRT = Shader.PropertyToID("_TempVSMRT");
                _blurCmd.GetTemporaryRT(tempRT, resolution, resolution, 0, FilterMode.Bilinear, RenderTextureFormat.RGFloat);
                _blurCmd.Blit(_depthRT, tempRT, _blurMaterial, 0);
                _blurCmd.Blit(tempRT, _blurRT, _blurMaterial, 1);
                _blurCmd.ReleaseTemporaryRT(tempRT);

                _resolution = resolution;
            }
            public void Resize(int resolution)
            {
                if (_resolution != resolution)
                {
                    _depthRT.Release();
                    _depthRT.width = resolution;
                    _depthRT.height = resolution;
                    _depthRT.Create();
                    _blurRT.Release();
                    _blurRT.width = resolution;
                    _blurRT.height = resolution;
                    _blurRT.Create();
                    _blurCmd.Clear();
                    int tempRT = Shader.PropertyToID("_TempVSMRT");
                    _blurCmd.GetTemporaryRT(tempRT, resolution, resolution, 0, FilterMode.Bilinear, RenderTextureFormat.RGFloat);
                    _blurCmd.Blit(_depthRT, tempRT, _blurMaterial, 0);
                    _blurCmd.Blit(tempRT, _blurRT, _blurMaterial, 1);
                    _blurCmd.ReleaseTemporaryRT(tempRT);
                    _resolution = resolution;
                }
            }
            public void DetachCmd(Camera camera)
            {
                camera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _blurCmd);
            }
            public void AttachCmd(Camera camera)
            {
                camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _blurCmd);
            }

            public RenderTexture targetRT { get { return _depthRT; } }
            public RenderTexture shadowMap { get { return _blurRT; } }
            public int resolution { get { return _resolution; } }

        }
        VSMData _vsmData = null;
        private VSMData vsmData
        {
            get
            {
                if (_vsmData == null)
                {
                    _vsmData = new VSMData(_shadowBlurMaterial, shadowResolution);
                }
                _vsmData.Resize(shadowResolution);
                return _vsmData;
            }
        }

        private RenderTexture _shadowMap = null;
        public RenderTexture shadowMap
        {
            get
            {
                if (shadowType == ShadowType.VSM)
                {
                    return vsmData.shadowMap;
                }
                else
                {
                    SetupShadowMap();
                    return _shadowMap;
                }
            }
        }
        private RenderTexture targetRT {
            get {
                if (shadowType == ShadowType.VSM)
                {
                    return vsmData.targetRT;
                }
                else
                {
                    SetupShadowMap();
                    return _shadowMap;
                }
            }
        }
        void SetupShadowMap()
        {
            if (_shadowMap == null)
            {
                _shadowMap = new RenderTexture(shadowResolution, shadowResolution, 16, RenderTextureFormat.Shadowmap);
                _shadowMap.Create();
            }
            else if (_shadowMap.width != shadowResolution || _shadowMap.height != shadowResolution)
            {
                _shadowMap.Release();
                _shadowMap.width = shadowResolution;
                _shadowMap.height = shadowResolution;
                _shadowMap.Create();
            }
        }

        private Camera _eyeCamera = null;
        private bool _isBlurCmdAttached = false;
        void SetupCamera()
        {
            if (_eyeCamera == null)
            {
                _eyeCamera = gameObject.AddComponent<Camera>();
                _eyeCamera.depth = Camera.main.depth - 1;
                _eyeCamera.nearClipPlane = shadowNearPlane;
                _eyeCamera.farClipPlane = shadowFarPlane;
            }
            _eyeCamera.targetTexture = targetRT;
            if (shadowType == ShadowType.VSM)
            {
                _eyeCamera.SetReplacementShader(depthShaderVSM, "RenderType");
                _eyeCamera.clearFlags = CameraClearFlags.Depth | CameraClearFlags.SolidColor;
                _eyeCamera.backgroundColor = new Color(0, 0, 0, 1);
                if (!_isBlurCmdAttached)
                {
                    vsmData.AttachCmd(_eyeCamera);
                    _isBlurCmdAttached = true;
                }
            }
            else
            {
                _eyeCamera.SetReplacementShader(depthShaderDefault, "RenderType");
                _eyeCamera.clearFlags = CameraClearFlags.Depth;
                if (_isBlurCmdAttached)
                {
                    vsmData.DetachCmd(_eyeCamera);
                    _isBlurCmdAttached = false;
                }
            }
            if (type == BasicLightType.Directional)
            {
                _eyeCamera.orthographic = true;
                _eyeCamera.orthographicSize = range;
            }
            else if (type == BasicLightType.Spot)
            {
                _eyeCamera.orthographic = false;
                _eyeCamera.fieldOfView = spotAngle;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        public Camera eyeCamera
        {
            get
            {
                SetupCamera();
                return _eyeCamera;
            }
        }

        public Vector3 position { get { return transform.position; } }
        public Vector3 direction { get { return transform.forward; } }
        public Matrix4x4 viewMatrix { get { return eyeCamera.worldToCameraMatrix; } }
        public Matrix4x4 projMatrix { get { return eyeCamera.projectionMatrix; } }
        void Update()
        {

            if (_eyeCamera != null) { _eyeCamera.enabled = (shadowType != ShadowType.None);
                _eyeCamera.nearClipPlane = shadowNearPlane;
                _eyeCamera.farClipPlane = shadowFarPlane;
            }
        }
        void OnDestroy() { if (_shadowMap) { _shadowMap.Release(); } }
    }
}