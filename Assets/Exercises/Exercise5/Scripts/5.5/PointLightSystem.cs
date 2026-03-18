using System.Linq;
using UnityEngine;

namespace Exercise5
{
    public class PointLightSystem : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Update()
        {
            PointLight light = FindFirstObjectByType<PointLight>();
            if (light != null)
            {
                Shader.SetGlobalVector("_LightPosition", light.position);
                Shader.SetGlobalVector("_LightDirection", light.direction);
                Shader.SetGlobalFloat("_LightIntensity", light.intensity);
                Shader.SetGlobalColor("_LightColor", light.color);
                Shader.SetGlobalMatrix("_LightViewMatrix", light.viewMatrix);
                var shadowMapType = 0;
                if (light.useShadow)
                {
                    shadowMapType = light.shadowMapType == PointLight.ShadowMapType.OmniDirectional ? 1 : 2;
                }
                Shader.SetGlobalInt("_LightShadowMapType", shadowMapType);
                Shader.SetGlobalFloat("_LightShadowBias", light.shadowBias);
                Shader.SetGlobalFloat("_LightShadowNearPlane", light.shadowNearPlane);
                Shader.SetGlobalFloat("_LightShadowFarPlane", light.shadowFarPlane);
                for (int i = 0; i < light.shadowMaps.Count(); i++)
                {
                    Shader.SetGlobalTexture("_LightShadowMap_" + i, light.shadowMaps[i]);
                }
            }
        }
    }
}
