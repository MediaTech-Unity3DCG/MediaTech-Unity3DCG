using UnityEngine;

namespace Chapter5.Sec5_4
{
    public enum BasicLightType { Point = 0, Directional = 1, Spot = 2 }
    public class BasicLight : MonoBehaviour
    {
        [SerializeField]
        public BasicLightType type = BasicLightType.Point;
        public Vector3 position { get { return transform.position; } }
        public Vector3 direction { get { return transform.forward; } }
        [SerializeField] public float     intensity  = 1.0f;
        [SerializeField] public Color     color      = Color.white;
        [SerializeField] public float     spotAngle  = 30.0f;
        [SerializeField] public float     range      = 10.0f;
        [SerializeField] public Texture2D cookie     = null;

        [SerializeField] public bool   useShadow         = false;
        [SerializeField] public int    shadowResolution  = 1024;
        [SerializeField,Range(0,1)] public float  shadowBias = 0.005f;
        private RenderTexture _shadowMap = null;
        public RenderTexture shadowMap{
            get{
                SetupShadowMap();
                return _shadowMap;
            }
        }
        void  SetupShadowMap()
        {
            if (_shadowMap == null){
                _shadowMap = new RenderTexture(shadowResolution, shadowResolution, 16, RenderTextureFormat.Shadowmap);
                _shadowMap.Create();
            }
            else if (_shadowMap.width != shadowResolution || _shadowMap.height != shadowResolution){
                _shadowMap.Release();
                _shadowMap.width  = shadowResolution;
                _shadowMap.height = shadowResolution;
                _shadowMap.Create();
            }
        }
        private Camera _eyeCamera = null;
        void  SetupCamera(){
            if (_eyeCamera == null){
                _eyeCamera    = gameObject.AddComponent<Camera>();
                Shader shader = Shader.Find("Unlit/Chapter5/BasicLightDepth");
                _eyeCamera.SetReplacementShader(shader, "RenderType");
                _eyeCamera.clearFlags = CameraClearFlags.Depth;
                _eyeCamera.depth = Camera.main.depth - 1;
                _eyeCamera.targetTexture = shadowMap;
            }
            if (type == BasicLightType.Directional){
                _eyeCamera.orthographic     = true;
                _eyeCamera.orthographicSize = range;
            }else if (type == BasicLightType.Spot){
                _eyeCamera.orthographic = false;
                _eyeCamera.fieldOfView  = spotAngle;
            }else{
                throw new System.NotImplementedException();
            }
        }
        public Camera eyeCamera{
            get{
                SetupCamera();
                return _eyeCamera;
            }
        }
        public Matrix4x4 viewMatrix { get { return eyeCamera.worldToCameraMatrix; } }
        public Matrix4x4 projMatrix { get { return eyeCamera.projectionMatrix; } }
        void Update(){if (_eyeCamera != null){_eyeCamera.enabled = useShadow;}}
        void OnDestroy(){if (_shadowMap){_shadowMap.Release();}}

    }
}