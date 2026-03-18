using UnityEngine;
using UnityEngine.Rendering;

namespace Exercise5
{
    public class AreaLightSystem : MonoBehaviour
    {
        [SerializeField] private RayTracingShader _rayTracingShader = null;
        private RayTracingAccelerationStructure _rayTracingAS = null;
        private CommandBuffer _commandBuffer = null;
        private RenderTexture _outputTexture = null;
        private RenderTexture _accumTexture = null;
        private bool _commandBufferAttached = false;
        private uint _cameraWidth = 0;
        private uint _cameraHeight = 0;
        private uint _frameCount = 0;
        [SerializeField]
        private bool _enableAccumulation = false;
        private bool _prevEnableAccumulation = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void CreateAccelerationStructure()
        {
            if (_rayTracingAS == null)
            {
                var settings = new RayTracingAccelerationStructure.Settings();
                settings.rayTracingModeMask = RayTracingAccelerationStructure.RayTracingModeMask.Everything;
                settings.managementMode = RayTracingAccelerationStructure.ManagementMode.Automatic;
                settings.layerMask = 255;
                _rayTracingAS = new RayTracingAccelerationStructure(settings);
            }
        }
        void CreateOutputTexture()
        {
            if (_cameraWidth != (uint)Camera.main.pixelWidth || _cameraHeight != (uint)Camera.main.pixelHeight || _outputTexture == null)
            {
                _cameraWidth = (uint)Camera.main.pixelWidth;
                _cameraHeight = (uint)Camera.main.pixelHeight;
                if (_outputTexture != null) { _outputTexture.Release(); }
                _outputTexture = new RenderTexture((int)_cameraWidth, (int)_cameraHeight, 0, RenderTextureFormat.ARGBFloat);
                _outputTexture.enableRandomWrite = true;
                _outputTexture.Create();
                _frameCount = 0;
            }
        }
        void CreateResources()
        {
            if (!SystemInfo.supportsRayTracing || _rayTracingShader == null)
            {
                Debug.LogError("Ray Tracing not supported or RayTracingShader is null.");
                return;
            }
            CreateAccelerationStructure();
            CreateOutputTexture();
            CreateAccumTexture();
        }
        void AttachCommandBuffer()
        {
            if (_commandBufferAttached) return;
            if (_commandBuffer == null) return;
            Camera.main.AddCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
            _commandBufferAttached = true;
        }
        void DetachCommandBuffer()
        {
            if (!_commandBufferAttached) return;
            if (_commandBuffer == null) return;
            Camera.main.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
            _commandBuffer.Release();
            _commandBuffer = null;
            _commandBufferAttached = false;
        }
        void SetupCommandBuffer()
        {
            DetachCommandBuffer();
            if (_rayTracingAS == null) return;
            var raygenName = _enableAccumulation ? "raygenMainAccum" : "raygenMainTemp";
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = "Ray Tracing Command";
            _commandBuffer.BuildRayTracingAccelerationStructure(_rayTracingAS);
            _commandBuffer.SetRayTracingShaderPass(_rayTracingShader, "DirectLighting");
            _commandBuffer.SetRayTracingAccelerationStructure(_rayTracingShader, "g_SceneAS", _rayTracingAS);
            _commandBuffer.SetRayTracingTextureParam(_rayTracingShader, "g_Output", _outputTexture);
            if (_enableAccumulation)
            {
                _commandBuffer.SetRayTracingTextureParam(_rayTracingShader, "g_Accum", _accumTexture);
                _commandBuffer.SetRayTracingIntParam(_rayTracingShader, "g_FrameCount", (int)_frameCount);

            }
            _commandBuffer.DispatchRays(_rayTracingShader, raygenName, _cameraWidth, _cameraHeight, 1, Camera.main);
            _commandBuffer.Blit(_outputTexture, BuiltinRenderTextureType.CameraTarget);
            _frameCount++;
            AttachCommandBuffer();
        }
        void OnValidate()
        {
            if (_enableAccumulation != _prevEnableAccumulation)
            {
                _prevEnableAccumulation = _enableAccumulation;
                _frameCount = 0;
            }
        }
        void Update()
        {
            CreateResources();
            SetupCommandBuffer();
        }
        void OnDestroy()
        {
            if (_commandBuffer != null)
            {
                _commandBuffer.Release();
            }
            if (_rayTracingAS != null)
            {
                _rayTracingAS.Release();
                _rayTracingAS = null;
            }
            if (_accumTexture != null)
            {
                _accumTexture.Release();
                _accumTexture = null;
            }
            if (_accumTexture != null)
            {
                _accumTexture.Release();
                _accumTexture = null;
            }
        }
        void CreateAccumTexture()
        {
            if (_frameCount == 0 && _enableAccumulation)
            {
                if (_accumTexture != null) { _accumTexture.Release(); _accumTexture = null; }
                _accumTexture = new RenderTexture((int)_cameraWidth, (int)_cameraHeight, 0, RenderTextureFormat.ARGBFloat);
                _accumTexture.enableRandomWrite = true;
                _accumTexture.Create();
            }
        }
    }
}