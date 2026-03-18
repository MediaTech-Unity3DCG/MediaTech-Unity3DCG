using UnityEngine;

namespace Chapter5.Sec5_3_1
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
            }
            else
            {
                Shader.SetGlobalInt("_LightType", 0);
            }
        }
    }
}
