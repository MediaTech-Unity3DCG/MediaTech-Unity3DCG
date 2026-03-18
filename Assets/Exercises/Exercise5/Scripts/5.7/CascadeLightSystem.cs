using UnityEngine;
using System.Collections.Generic;

namespace Exercise5
{
    public class CascadeLightSystem : MonoBehaviour
    {
        void Update()
        {
            CascadeLight light = FindFirstObjectByType<CascadeLight>();

            if (light != null)
            {
                // --- 既存の基本情報転送 (Type, Dir, Intensity, Color) ---
                Shader.SetGlobalInt("_LightType", 2);
                Shader.SetGlobalVector("_LightDirection", light.direction);
                Shader.SetGlobalFloat("_LightIntensity", light.intensity);
                Shader.SetGlobalColor("_LightColor", light.color);

                // --- カスケード情報の取得 ---
                List<float> splits = light.cascadeSplits;
                List<float> biases = light.cascadeBiases;
                int count = splits.Count;
                Shader.SetGlobalInt("_CascadeCount", count);
                Shader.SetGlobalInt("_VisualizeCascades", light.visualizeCascades ? 1 : 0);

                Vector4 cascadeDistances = Vector4.zero;
                Vector4 cascadeBiases = Vector4.zero;

                float currentFar = 0;
                float cameraNear = Camera.main.nearClipPlane;
                float cameraFar = Camera.main.farClipPlane;

                // --- ループで個別データを転送 ---
                for (int i = 0; i < count; i++)
                {
                    // 1. シャドウマップをセット
                    Shader.SetGlobalTexture($"_ShadowMap{i}", light.GetShadowMap(i));

                    // 2. 行列 (VP Matrix) をセット
                    // 各カスケードカメラの行列を個別のプロパティ名で送る
                    Shader.SetGlobalMatrix($"_ShadowVP{i}", light.GetShadowVPMatrix(i));

                    // 3. 距離とバイアスの計算
                    currentFar += (cameraFar - cameraNear) * splits[i];
                    cascadeDistances[i] = currentFar;
                    if (i < biases.Count) cascadeBiases[i] = biases[i];
                }

                // 4. まとめてVector4で転送
                Shader.SetGlobalVector("_CascadeDistances", cascadeDistances);
                Shader.SetGlobalVector("_CascadeBiases", cascadeBiases);
            }
            else
            {
                Shader.SetGlobalInt("_LightType", 0);
            }
        }
    }
}