using UnityEngine;

namespace Lize
{
    public sealed class BufferClearer
    {
        readonly ComputeShader clearShader;
        readonly int clearKernelIndex;
        readonly int clearThreadGroupSize;

        public BufferClearer(ComputeShader clearShader)
        {
            this.clearShader = clearShader;
            clearKernelIndex = clearShader.FindKernel("Clear");
            clearShader.GetKernelThreadGroupSizes(clearKernelIndex, out var x, out _, out _);
            clearThreadGroupSize = (int) x;
        }

        public void Clear(GraphicsBuffer buffer, int count)
        {
            clearShader.SetBuffer(clearKernelIndex, "_Target", buffer);
            clearShader.Dispatch(clearKernelIndex, Mathf.CeilToInt(count / (float) clearThreadGroupSize), 1, 1);
        }
    }
}
