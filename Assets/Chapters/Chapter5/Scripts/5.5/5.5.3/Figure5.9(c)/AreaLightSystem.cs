using UnityEngine;
using UnityEngine.Rendering;

namespace Chapter5.Sec5_5.Figure5_9_C
{
    public class AreaLightSystem : MonoBehaviour
    {
        private CommandBuffer _commandBuffer = null;
        private bool _commandBufferAttached = false;
        private RenderTexture _outputTexture = null;
        private RayTracingAccelerationStructure _rayTracingAS = null;
        [SerializeField]
        private RayTracingShader _rayTracingShader = null;
        private uint _cameraWidth = 0;
        private uint _cameraHeight = 0;
        void CreateResources()
        {
            if (!SystemInfo.supportsRayTracing || !_rayTracingShader) { return; }
            CreateAccelerationStructure();
            CreateOutputTexture();
        }
        void CreateAccelerationStructure()
        {
            if (_rayTracingAS != null) { return; }
            var settings = new RayTracingAccelerationStructure.Settings();
            settings.managementMode =
            RayTracingAccelerationStructure.ManagementMode.Automatic;
            settings.layerMask = ~0;
            settings.rayTracingModeMask =
            RayTracingAccelerationStructure.RayTracingModeMask.Everything;
            _rayTracingAS = new RayTracingAccelerationStructure(settings);
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
            }
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
            if (!_rayTracingShader || _rayTracingAS == null || !_outputTexture) { return; }
            DetachCommandBuffer();
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = "Ray Tracing Command";
            _commandBuffer.BuildRayTracingAccelerationStructure(_rayTracingAS);
            _commandBuffer.SetRayTracingShaderPass(_rayTracingShader, "DirectLighting");
            _commandBuffer.SetRayTracingAccelerationStructure(_rayTracingShader, "g_SceneAS", _rayTracingAS);
            _commandBuffer.SetRayTracingTextureParam(_rayTracingShader, "g_Output", _outputTexture);
            _commandBuffer.DispatchRays(_rayTracingShader, "raygenMain", _cameraWidth, _cameraHeight, 1, Camera.main);
            _commandBuffer.Blit(_outputTexture, BuiltinRenderTextureType.CameraTarget);
            AttachCommandBuffer();
        }
        void Update() { CreateResources(); SetupCommandBuffer(); }
        void OnDestroy()
        {
            if (_commandBuffer != null) { _commandBuffer.Release(); }
            if (_rayTracingAS != null) { _rayTracingAS.Release(); }
            if (_outputTexture != null) { _outputTexture.Release(); }
        }
    }
}
