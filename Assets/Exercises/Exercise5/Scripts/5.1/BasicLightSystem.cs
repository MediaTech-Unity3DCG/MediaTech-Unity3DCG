using UnityEngine;

namespace Exercise5
{
    public class BasicLightSystem : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Update()
        {
            BasicLight light = FindFirstObjectByType<BasicLight>();
            if (light != null)
            {
                Shader.SetGlobalInt("_LightType", (int)light.type + 1);
                Shader.SetGlobalVector("_LightPosition", light.position);
                Shader.SetGlobalVector("_LightDirection", light.direction);
                Shader.SetGlobalFloat("_LightIntensity", light.intensity);
                Shader.SetGlobalColor("_LightColor", light.color);
                Shader.SetGlobalTexture("_LightCookie", light.cookie);
                if (light.type == BasicLightType.Spot ||
                    light.type == BasicLightType.Directional)
                {
                    var projMatrix = GL.GetGPUProjectionMatrix(light.projMatrix, false);
                    Shader.SetGlobalMatrix("_LightViewMatrix", light.viewMatrix);
                    Shader.SetGlobalMatrix("_LightProjMatrix", projMatrix);
                    Shader.SetGlobalInt("_LightUseShadow"    , light.useShadow ? 1 : 0);
                    Shader.SetGlobalTexture("_LightShadowMap", light.shadowMap);
                    Shader.SetGlobalFloat("_LightShadowBias", light.shadowBias);
                }
                else
                {
                    Shader.SetGlobalInt("_LightUseShadow", 0);
                }
            }
            else
            {
                Shader.SetGlobalInt("_LightType", 0);
            }
        }
    }
}
