using UnityEngine;

namespace Chapter5.Sec5_3_4
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
                var viewMatrix = light.transform.worldToLocalMatrix;
                var projMatrix = Matrix4x4.Perspective(light.spotAngle, 1.0f, 0.01f, light.range);
                Shader.SetGlobalMatrix("_LightViewMatrix", viewMatrix);
                Shader.SetGlobalMatrix("_LightProjMatrix", projMatrix);
            }
            else
            {
                Shader.SetGlobalInt("_LightType", 0);
            }
        }
    }
}
