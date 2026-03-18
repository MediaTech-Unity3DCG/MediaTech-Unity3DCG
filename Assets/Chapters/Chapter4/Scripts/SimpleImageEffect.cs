using UnityEngine;
using UnityEngine.Rendering;

namespace Chapter4
{
    [ExecuteInEditMode]
    public class SimpleImageEffect : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private Material _BlitMaterial;
        private CommandBuffer _BlitCommand;
        private Camera _MainCamera;

        private void OnEnable()
        {
            _MainCamera = Camera.main;
            if (_MainCamera == null)
            {
                Debug.LogError("SimpleImageEffect must be attached to a Camera.");
                enabled = false;
                return;
            }
            if (_BlitMaterial == null)
            {
                Debug.LogError("Blit Material is not assigned.");
                enabled = false;
                return;
            }
            _BlitCommand = new CommandBuffer { name = "Simple Image Effect Blit" };
            var tempRT = Shader.PropertyToID("_TempRT");
            _BlitCommand.GetTemporaryRT(tempRT, -1, -1, 0, FilterMode.Bilinear);
            _BlitCommand.Blit(BuiltinRenderTextureType.CameraTarget, tempRT);
            _BlitCommand.Blit(tempRT, BuiltinRenderTextureType.CameraTarget, _BlitMaterial);
            _BlitCommand.ReleaseTemporaryRT(tempRT);
            _MainCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, _BlitCommand);
        }

        private void OnDisable()
        {
            if (_MainCamera != null && _BlitCommand != null)
            {
                _MainCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, _BlitCommand);
                _BlitCommand.Release();
                _BlitCommand = null;
            }
        }
    }
}
