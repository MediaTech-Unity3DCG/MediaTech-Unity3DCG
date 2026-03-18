using UnityEngine;

namespace Chapter6.Fluid
{
    using static ShaderProperties;

    public class FluidRenderer : MonoBehaviour
    {
        [SerializeField]
        private SPH2D solver = null;
        [SerializeField]
        private Material particleRenderer = null;
        [SerializeField]
        protected float radius = 0.05f;
        [SerializeField]
        protected float clipThreshold = 0.1f;
        [SerializeField]
        protected Color particleColor = Color.blue;

        private Material material = null;


        private void Awake()
        {
            this.material = new Material(this.particleRenderer);
        }

        private void OnRenderObject()
        {
            this.material.SetPass(0);
            this.material.SetFloat(PropRadius, this.radius);
            this.material.SetFloat(PropClipThreshold, this.clipThreshold);
            this.material.SetColor(PropParticleColor, this.particleColor);
            this.material.SetBuffer(PropParticlesBuffer, this.solver.ParticlesBuffer);
            Graphics.DrawProceduralNow(MeshTopology.Points, this.solver.ParticlesBuffer.count);
        }

        private void OnDestroy()
        {
            if (this.material)
            {
                Destroy(this.material);
            }
        }
    }
}
