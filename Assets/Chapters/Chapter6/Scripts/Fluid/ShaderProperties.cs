using UnityEngine;

namespace Chapter6.Fluid
{
    public static class ShaderProperties
    {
        // Buffers.
        public static readonly int PropParticlesBuffer = Shader.PropertyToID("_ParticlesBuffer");

        // Parameters.
        public static readonly int PropRandSeed = Shader.PropertyToID("_RandSeed");
        public static readonly int PropInitRange = Shader.PropertyToID("_InitRange");
        public static readonly int PropRandUnitRadius = Shader.PropertyToID("_RandUnitRadius");
        public static readonly int PropNumParticles = Shader.PropertyToID("_NumParticles");
        public static readonly int PropTimeStep = Shader.PropertyToID("_TimeStep");
        public static readonly int PropSmoothLen = Shader.PropertyToID("_SmoothLen");
        public static readonly int PropPressureStiffness = Shader.PropertyToID("_PressureStiffness");
        public static readonly int PropRestDensity = Shader.PropertyToID("_RestDensity");
        public static readonly int PropDensityCoe = Shader.PropertyToID("_DensityCoe");
        public static readonly int PropGradPressureCoe = Shader.PropertyToID("_GradPressureCoe");
        public static readonly int PropLapViscosityCoe = Shader.PropertyToID("_LapViscosityCoe");
        public static readonly int PropWallStiffness = Shader.PropertyToID("_WallStiffness");
        public static readonly int PropViscosity = Shader.PropertyToID("_Viscosity");
        public static readonly int PropGravity = Shader.PropertyToID("_Gravity");
        public static readonly int PropRange = Shader.PropertyToID("_Range");
        public static readonly int PropParticleColor = Shader.PropertyToID("_ParticleColor");
        public static readonly int PropRadius = Shader.PropertyToID("_Radius");
        public static readonly int PropClipThreshold = Shader.PropertyToID("_ClipThreshold");

        // Kernels.
        public static int InitKernelID = -1;
        public static int PropertyKernelID = -1;
        public static int ForceKernelID = -1;
        public static int IntegrateKernelID = -1;
        public const string InitKernel = "init_cs";
        public const string PropertyKernel = "property_cs";
        public const string ForceKernel = "force_cs";
        public const string IntegrateKernel = "integrate_cs";


        public static void UpdateKernelID(ComputeShader fluidCS)
        {
            InitKernelID = fluidCS.FindKernel(InitKernel);
            PropertyKernelID = fluidCS.FindKernel(PropertyKernel);
            ForceKernelID = fluidCS.FindKernel(ForceKernel);
            IntegrateKernelID = fluidCS.FindKernel(IntegrateKernel);
        }
    }
}
