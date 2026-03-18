using UnityEngine;
using UnityEngine.Rendering;

namespace Chapter5.Sec5_5.Figure5_7
{
    public class AreaLightSystem : MonoBehaviour
    {
        private CommandBuffer _commandBuffer = null;
        private bool _commandBufferAttached = false;
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
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = "Clear Command";
            _commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            _commandBuffer.ClearRenderTarget(true, true, Color.red);
            AttachCommandBuffer();
        }
        void Update() { SetupCommandBuffer(); }
        void OnDestroy() { if (_commandBuffer != null) { _commandBuffer.Release(); } }
    }
}
