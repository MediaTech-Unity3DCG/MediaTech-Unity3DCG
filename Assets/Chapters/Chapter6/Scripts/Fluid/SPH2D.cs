using System.Runtime.InteropServices;
using UnityEngine;

namespace Chapter6.Fluid
{
    using static ShaderProperties;

    public class SPH2D : MonoBehaviour
    {
        private const int ThreadSizeX = 256;

        public GraphicsBuffer ParticlesBuffer { get; private set; } = null;

        [SerializeField]
        private ComputeShader fluidCS = null;
        [SerializeField, Tooltip("パーティクルの個数"), Range(0, ThreadSizeX * 500)]
        private int particleCount = ThreadSizeX * 4;
        [SerializeField, Tooltip("粒子半径")]
        private float smoothLen = 0.5f;
        [SerializeField, Tooltip("圧力項係数")]
        private float pressureStiffness = 0.57f;
        [SerializeField, Tooltip("静止密度")]
        private float restDensity = 4f;
        [SerializeField, Tooltip("粒子質量")]
        private float particleMass = 0.08f;
        [SerializeField, Tooltip("粘性係数")]
        private float viscosity = 3f;
        [SerializeField, Tooltip("時間刻み幅")]
        private float maxAllowableTimestep = 0.005f;
        [SerializeField, Tooltip("ペナルティ法の壁の力")]
        private float wallStiffness = 3000f;
        [SerializeField, Tooltip("イテレーション回数")]
        private int iterations = 4;
        [SerializeField, Tooltip("重力")]
        private Vector2 gravity = new(0.0f, -9.8f);
        [SerializeField, Tooltip("シミュレーション空間")]
        private Vector2 range = new(16, 9);
        [SerializeField, Tooltip("粒子位置初期化時の円半径")]
        private float randUnitRadius = 5f;

        private float timeStep = 0f;
        private float densityCoe = 0f; // Poly6カーネルの密度係数
        private float gradPressureCoe = 0f; // Spikyカーネルの圧力係数
        private float lapViscosityCoe = 0f; // Laplacianカーネルの粘性係数


        private void Start()
        {
            UpdateKernelID(this.fluidCS);

            this.ParticlesBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured, this.particleCount, Marshal.SizeOf(typeof(FluidParticle))
            );
            
            var threadGroupsX = Mathf.CeilToInt((float)this.particleCount / ThreadSizeX);
            this.fluidCS.SetInt(PropNumParticles, this.particleCount);
            this.fluidCS.SetInt(PropRandSeed, Random.Range(0, int.MaxValue));
            this.fluidCS.SetVector(PropInitRange, 0.5f * this.range);
            this.fluidCS.SetFloat(PropRandUnitRadius, this.randUnitRadius);
            this.fluidCS.SetBuffer(InitKernelID, PropParticlesBuffer, this.ParticlesBuffer);
            this.fluidCS.Dispatch(InitKernelID, threadGroupsX, 1, 1);
        }

        private void Update()
        {
            this.timeStep = Mathf.Min(this.maxAllowableTimestep, Time.deltaTime);

            // 2Dカーネル係数
            this.densityCoe = this.particleMass * 4f / (Mathf.PI * Mathf.Pow(this.smoothLen, 8f));
            this.gradPressureCoe = this.particleMass * -30f / (Mathf.PI * Mathf.Pow(this.smoothLen, 5f));
            this.lapViscosityCoe = this.particleMass * 20f / (3f * Mathf.PI * Mathf.Pow(this.smoothLen, 5f));

            // Const.
            this.fluidCS.SetInt(PropNumParticles, this.particleCount);
            this.fluidCS.SetFloat(PropTimeStep, this.timeStep);
            this.fluidCS.SetFloat(PropSmoothLen, this.smoothLen);
            this.fluidCS.SetFloat(PropPressureStiffness, this.pressureStiffness);
            this.fluidCS.SetFloat(PropRestDensity, this.restDensity);
            this.fluidCS.SetFloat(PropViscosity, this.viscosity);
            this.fluidCS.SetFloat(PropDensityCoe, this.densityCoe);
            this.fluidCS.SetFloat(PropGradPressureCoe, this.gradPressureCoe);
            this.fluidCS.SetFloat(PropLapViscosityCoe, this.lapViscosityCoe);
            this.fluidCS.SetFloat(PropWallStiffness, this.wallStiffness);
            this.fluidCS.SetVector(PropRange, this.range);
            this.fluidCS.SetVector(PropGravity, this.gravity);

            for (var i = 0; i < this.iterations; i++)
            {
                this.ExecuteFluidSolver();
            }
        }

        private void OnDestroy()
        {
            this.ParticlesBuffer?.Dispose();
        }

        private void ExecuteFluidSolver()
        {
            var threadGroupsX = Mathf.CeilToInt((float)this.particleCount / ThreadSizeX);

            // Property
            this.fluidCS.SetBuffer(PropertyKernelID, PropParticlesBuffer, this.ParticlesBuffer);
            this.fluidCS.Dispatch(PropertyKernelID, threadGroupsX, 1, 1);

            // Force
            this.fluidCS.SetBuffer(ForceKernelID, PropParticlesBuffer, this.ParticlesBuffer);
            this.fluidCS.Dispatch(ForceKernelID, threadGroupsX, 1, 1);

            // Integrate
            this.fluidCS.SetBuffer(IntegrateKernelID, PropParticlesBuffer, this.ParticlesBuffer);
            this.fluidCS.Dispatch(IntegrateKernelID, threadGroupsX, 1, 1);
        }
    }
}
