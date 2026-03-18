using UnityEngine;

namespace Chapter4
{
    public class IBLManager : MonoBehaviour
    {
        [SerializeField] private Material _Skybox;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            if (!_Skybox)
            {
                return;
            }
            RenderSettings.skybox = _Skybox;
            Shader.SetGlobalTexture("_Envmap", _Skybox.GetTexture("_Tex"));
        }
    }
}
