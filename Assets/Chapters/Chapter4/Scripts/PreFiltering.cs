using UnityEngine;
using UnityEngine.Rendering;

namespace Chapter4
{
    public class PreFiltering : MonoBehaviour
    {
        [SerializeField] private int _Width = 512;
        [SerializeField] private int _MipCount = 0;
        [SerializeField] private int _MaxSamples = 256;
        [SerializeField] private Material _PrefilteringDiffuseMaterial;
        [SerializeField] private Material _PrefilteringSpecularMaterial;
        [SerializeField] private Material _PrefilteringSpecularLutMaterial;
        [SerializeField] private RenderTexture _DiffuseTexture;
        [SerializeField] private RenderTexture _SpecularTexture;
        [SerializeField] private RenderTexture _SpecularLutTexture;

        private void InitTextures()
        {
            ReleaseTextures();
            if (_PrefilteringDiffuseMaterial)
            {
                var format = RenderTextureFormat.ARGBHalf;
                var rtDesc = new RenderTextureDescriptor(_Width, _Width, format, 0);
                rtDesc.dimension = TextureDimension.Cube;
                rtDesc.mipCount = 1;
                _DiffuseTexture = new RenderTexture(rtDesc);
                _DiffuseTexture.Create();
                rtDesc.width = 6 * _Width;
                rtDesc.dimension = TextureDimension.Tex2D;
                var tempRT = RenderTexture.GetTemporary(rtDesc);
                _PrefilteringDiffuseMaterial.SetInt("_Width", _Width);
                _PrefilteringDiffuseMaterial.SetInt("_MaxSamples", _MaxSamples);
                Graphics.Blit(null, tempRT, _PrefilteringDiffuseMaterial);
                for (int i = 0; i < 6; i++)
                {
                    int offset = i * _Width;
                    Graphics.CopyTexture(tempRT, 0, 0, offset, 0, _Width,
                        _Width, _DiffuseTexture, i, 0, 0, 0
                    );
                }
                RenderTexture.ReleaseTemporary(tempRT);
                Shader.SetGlobalTexture("_DiffuseEnvmap", _DiffuseTexture);
            }
            if (_PrefilteringSpecularMaterial)
            {
                var format = RenderTextureFormat.ARGBHalf;
                var rtDesc = new RenderTextureDescriptor(_Width, _Width, format, 0);
                rtDesc.dimension = TextureDimension.Cube;
                rtDesc.mipCount = _MipCount;
                rtDesc.useMipMap = true;
                rtDesc.autoGenerateMips = false;
                _SpecularTexture = new RenderTexture(rtDesc);
                _SpecularTexture.Create();
                int width = _Width;
                rtDesc.dimension = TextureDimension.Tex2D;
                rtDesc.width = 6 * width;
                rtDesc.useMipMap = false;
                var tempRT = RenderTexture.GetTemporary(rtDesc);
                for (int j = 0; j < _MipCount; ++j)
                {
                    float roughness = (float)j / (_MipCount - 1);
                    _PrefilteringSpecularMaterial.SetInt("_Width", width);
                    _PrefilteringSpecularMaterial.SetInt("_MaxSamples", _MaxSamples);
                    _PrefilteringSpecularMaterial.SetFloat("_Roughness", roughness);
                    Graphics.Blit(null, tempRT, _PrefilteringSpecularMaterial);
                    for (int i = 0; i < 6; i++)
                    {
                        Graphics.CopyTexture(tempRT, 0, 0, width * i, 0, width, width, _SpecularTexture, i, j, 0, 0);
                    }
                    width = Mathf.Max(width / 2, 1);
                }
                RenderTexture.ReleaseTemporary(tempRT);
                Shader.SetGlobalTexture("_SpecularEnvmap", _SpecularTexture);
            }
            if (_PrefilteringSpecularLutMaterial)
            {
                var format = RenderTextureFormat.ARGBHalf;
                var rtDesc = new RenderTextureDescriptor(_Width, _Width, format, 0);
                rtDesc.dimension = TextureDimension.Tex2D;
                rtDesc.useMipMap = false;
                _SpecularLutTexture = new RenderTexture(rtDesc);
                _SpecularLutTexture.Create();
                _PrefilteringSpecularLutMaterial.SetInt("_Width", _Width);
                Graphics.Blit(null, _SpecularLutTexture, _PrefilteringSpecularLutMaterial);
                Shader.SetGlobalTexture("_SpecularBrdfLut", _SpecularLutTexture);
            }
        }

        private void ReleaseTextures()
        {
            if (_DiffuseTexture != null)
            {
                _DiffuseTexture.Release();
                Destroy(_DiffuseTexture);
                _DiffuseTexture = null;
            }
            if (_SpecularTexture != null)
            {
                _SpecularTexture.Release();
                Destroy(_SpecularTexture);
                _SpecularTexture = null;
            }
            if (_SpecularLutTexture != null)
            {
                _SpecularLutTexture.Release();
                Destroy(_SpecularLutTexture);
                _SpecularLutTexture = null;
            }
        }

        void OnDestroy()
        {
            ReleaseTextures();
        }

        void Start()
        {
            InitTextures();
        }
    }
}
