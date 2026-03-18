using System.Runtime.InteropServices;
using UnityEngine;

namespace Chapter6
{
    public class Test : MonoBehaviour
    {
        public ComputeShader CS = null;
        public Vector2Int TexSize = new(16, 16);

        private RenderTexture rt = null;
        private GraphicsBuffer buffer = null;

        private void Awake()
        {
            this.buffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured, 4, Marshal.SizeOf(typeof(Vector4))
            );
            this.buffer.SetData(new[]
                {
                    new Vector4(0f, 0f, 0f, 1f),
                    new Vector4(0.3f, 0.3f, 0.3f, 1f),
                    new Vector4(0.6f, 0.6f, 0.6f, 1f),
                    new Vector4(1f, 1f, 1f, 1f)
                }
            );

            this.rt = new RenderTexture(
                this.TexSize.x, this.TexSize.y, 0,
                RenderTextureFormat.ARGB32
            )
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };
            this.rt.Create();

            var kernel = this.CS.FindKernel("CSMain");
            this.CS.GetKernelThreadGroupSizes(kernel, out var x, out var y, out _);
            
            this.CS.SetBuffer(kernel, "Buff", this.buffer);
            this.CS.SetTexture(kernel, "Tex", this.rt);
            this.CS.Dispatch(kernel, 2, 2, 1);
        }

        private void OnGUI()
        {
            if (this.rt == null)
            {
                return;
            }

            GUI.DrawTexture(
                new Rect(0, 0, Screen.width, Screen.height),
                this.rt
            );
        }

        private void OnDestroy()
        {
            this.rt?.Release();
            this.buffer?.Release();
        }
    }
}
