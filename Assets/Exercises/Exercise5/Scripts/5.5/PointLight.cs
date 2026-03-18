using System.Collections.Generic;
using UnityEngine;

namespace Exercise5
{
    public class PointLight : MonoBehaviour
    {
        public enum ShadowMapType
        {
            OmniDirectional,
            DualParaboloid
        }
        public Vector3 position { get { return transform.position; } }
        public Vector3 direction { get { return transform.forward; } }
        [SerializeField] public float intensity = 1.0f;
        [SerializeField] public Color color = Color.white;
        [SerializeField] public float range = 10.0f;
        [SerializeField] public bool useShadow = false;
        [SerializeField] public int shadowResolution = 1024;
        [SerializeField, Range(0, 1)] public float shadowBias = 0.005f;
        [SerializeField] public float shadowNearPlane = 0.1f;
        [SerializeField] public float shadowFarPlane = 1000.0f;
        [SerializeField] public ShadowMapType shadowMapType = ShadowMapType.OmniDirectional;
        [SerializeField] private bool _useDepthWithTesselation = false;
        private Shader _depthShaderOmniDirectional = null;
        private Shader _depthShaderDualParaboloid = null;
        private Shader depthShaderOmniDirectional
        {
            get
            {
                if (_depthShaderOmniDirectional == null)
                {
                    _depthShaderOmniDirectional = Shader.Find("Unlit/Exercise5/BasicLightDepth");
                }
                return _depthShaderOmniDirectional;
            }
        }
        private Shader depthShaderDualParaboloid
        {
            get
            {
                if (_depthShaderDualParaboloid == null)
                {
                    _depthShaderDualParaboloid = Shader.Find(_useDepthWithTesselation ? "Unlit/Exercise5/ParaboloidLightDepthWithTessellation" : "Unlit/Exercise5/ParaboloidLightDepth");
                }
                return _depthShaderDualParaboloid;
            }
        }

        private class FaceObject
        {
            // GameObject representing the face
            private GameObject _gameObject = null;
            private Camera _eyeCamera = null;
            private Shader _depthShaderOmniDirectional = null;
            private Shader _depthShaderDualParaboloid = null;
            private RenderTexture _shadowMap = null;
            private ShadowMapType _shadowMapType = ShadowMapType.OmniDirectional;
            private int _faceIndex = 0;
            private bool _useShadow = false;
            private float _nearClipPlane = 0.1f;
            private float _farClipPlane = 1000.0f;
            private int _shadowResolution = 0;

            public FaceObject(Transform parent, Shader depthShaderOmniDirectional, Shader depthShaderDualParaboloid, ShadowMapType shdMapType, int faceIndex, int shadowRes, float near, float far)
            {
                _gameObject = new GameObject("PointLight_Face_" + faceIndex);
                _gameObject.transform.parent = parent;
                _depthShaderOmniDirectional = depthShaderOmniDirectional;
                _depthShaderDualParaboloid = depthShaderDualParaboloid;
                _faceIndex = faceIndex;
                _nearClipPlane = near;
                _farClipPlane = far;
                _shadowResolution = shadowRes;
                _shadowMapType = shdMapType;
                SetupShadowMapType();
            }
            void SetupShadowMapType()
            {
                if (_shadowMapType == ShadowMapType.OmniDirectional)
                {
                    // Set rotation based on face index
                    switch (_faceIndex)
                    {
                        case 0:
                            // U:-Z V:+Y
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                            break;
                        case 1:
                            // U:+Z V:+Y
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
                            break;
                        case 2:
                            // U:+X V:-Z
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
                            break;
                        case 3:
                            // U:+X V:+Z
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                            break;
                        case 4:
                            // U:+X V:+Y
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                            break;
                        case 5:
                            // U:-X V:+Y
                            _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                            break;
                    }
                }
                else
                {
                    // Dual Paraboloid uses the same orientation for both faces
                    // Face 0: Front paraboloid
                    // Face 1: Back paraboloid
                    if (_faceIndex == 0)
                    {
                        _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.forward, -Vector3.up);
                    }
                    else if (_faceIndex == 1)
                    {
                        // Face 1: Back paraboloid
                        _gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.back, -Vector3.up);
                    }
                }
                if (_eyeCamera != null)
                {
                    SetupCamera();
                }
            }
            public ShadowMapType shadowMapType
            {
                get { return _shadowMapType; }
                set
                {
                    if (_shadowMapType == value)
                    {
                        return;
                    }
                    _shadowMapType = value;
                    SetupShadowMapType();
                }
            }
            public bool useShadow
            {
                get { return _useShadow; }
                set
                {
                    if (_useShadow == value)
                    {
                        return;
                    }
                    _useShadow = value;
                    if (_useShadow)
                    {
                        _gameObject.SetActive(true);
                        SetupCamera();
                    }
                    else
                    {
                        _gameObject.SetActive(false);
                    }
                }
            }
            void SetupCamera()
            {
                if (_eyeCamera == null)
                {
                    _eyeCamera = _gameObject.AddComponent<Camera>();
                    _eyeCamera.orthographic = false;
                    _eyeCamera.clearFlags = CameraClearFlags.Depth;
                    _eyeCamera.depth = Camera.main.depth - 1;
                    _eyeCamera.fieldOfView = 90.0f;
                    _eyeCamera.targetTexture = shadowMap;
                }
                var depthShader = _shadowMapType == ShadowMapType.OmniDirectional ? _depthShaderOmniDirectional : _depthShaderDualParaboloid;
                _eyeCamera.SetReplacementShader(depthShader, "RenderType");
                _eyeCamera.nearClipPlane = nearClipPlane;
                _eyeCamera.farClipPlane = farClipPlane;
                _eyeCamera.enabled = _shadowMapType == ShadowMapType.OmniDirectional ? true : _faceIndex < 2;
            }
            public Camera eyeCamera
            {
                get
                {
                    SetupCamera();
                    return _eyeCamera;
                }
            }
            void SetupShadowMap()
            {
                if (_shadowMap == null)
                {
                    _shadowMap = new RenderTexture(shadowResolution, shadowResolution, 32, RenderTextureFormat.Shadowmap);
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
            public RenderTexture shadowMap
            {
                get
                {
                    SetupShadowMap();
                    return _shadowMap;
                }

            }
            public float nearClipPlane
            {
                get { return _nearClipPlane; }
                set
                {
                    if (_nearClipPlane == value)
                    {
                        return;
                    }
                    _nearClipPlane = value;
                    if (_eyeCamera != null)
                    {
                        _eyeCamera.nearClipPlane = _nearClipPlane;
                    }
                }
            }
            public float farClipPlane
            {
                get { return _farClipPlane; }
                set
                {
                    if (_farClipPlane == value)
                    {
                        return;
                    }
                    _farClipPlane = value;
                    if (_eyeCamera != null)
                    {
                        _eyeCamera.farClipPlane = _farClipPlane;
                    }
                }
            }
            public int shadowResolution
            {
                get { return _shadowResolution; }
                set
                {
                    if (_shadowResolution == value)
                    {
                        return;
                    }
                    _shadowResolution = value;
                    if (_shadowMap != null)
                    {
                        if (_shadowMap.width != shadowResolution || _shadowMap.height != shadowResolution)
                        {
                            _shadowMap.Release();
                            _shadowMap.width = shadowResolution;
                            _shadowMap.height = shadowResolution;
                            _shadowMap.Create();
                        }
                    }
                }
            }
            public void Destroy()
            {
                if (_eyeCamera != null)
                {
                    _eyeCamera.targetTexture = null;
                    _eyeCamera = null;
                }
                if (_shadowMap != null)
                {
                    _shadowMap.Release();
                    Object.DestroyImmediate(_shadowMap);
                    _shadowMap = null;
                }
                if (_gameObject != null)
                {
                    Object.DestroyImmediate(_gameObject);
                    _gameObject = null;
                }
            }
        }
        private FaceObject[] _faces = null;
        private FaceObject[] faces
        {
            get
            {
                if (_faces == null && !useShadow)
                {
                    return null;
                }
                if (_faces == null)
                {
                    _faces = new FaceObject[6];
                    for (int i = 0; i < 6; i++)
                    {
                        _faces[i] = new FaceObject(transform, depthShaderOmniDirectional, depthShaderDualParaboloid, shadowMapType, i, shadowResolution, shadowNearPlane, shadowFarPlane);
                    }
                }
                return _faces;
            }
        }
        public Matrix4x4 viewMatrix { get { return transform.worldToLocalMatrix; } }
        public List<RenderTexture> shadowMaps
        {
            get
            {
                List<RenderTexture> maps = new List<RenderTexture>();
                if (faces == null || !useShadow)
                {
                    return maps;
                }
                if (shadowMapType == ShadowMapType.OmniDirectional)
                {
                    maps.Add(faces[0].shadowMap);
                    maps.Add(faces[1].shadowMap);
                    maps.Add(faces[2].shadowMap);
                    maps.Add(faces[3].shadowMap);
                    maps.Add(faces[4].shadowMap);
                    maps.Add(faces[5].shadowMap);
                }
                else
                {
                    maps.Add(faces[0].shadowMap);
                    maps.Add(faces[1].shadowMap);
                }
                return maps;
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < _faces.Length; i++)
            {
                _faces[i].Destroy();
                _faces[i] = null;
            }
            _faces = null;
        }
        void Update()
        {
            if (faces == null)
            {
                return;
            }
            for (int i = 0; i < this.faces.Length; i++)
            {
                // 解像度を設定
                faces[i].useShadow = useShadow;
                faces[i].shadowMapType = shadowMapType;
                faces[i].shadowResolution = shadowResolution;
                faces[i].nearClipPlane = shadowNearPlane;
                faces[i].farClipPlane = shadowFarPlane;
            }
        }
    }
}