using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lize
{
    public sealed class Rasterizer : IDisposable
    {
        readonly ComputeShader clearShader;
        readonly ComputeShader rasterShader;
        public int Resolution { get; }
        public int Length { get; }

        public GraphicsBuffer TargetBuffer { get; private set; }
        int clearKernelIndex;
        int clearThreadGroupSize;

        int renderKernelIndex;
        int renderThreadGroupSize;

        public Rasterizer(ComputeShader clearShader, ComputeShader rasterShader, int resolution)
        {
            this.clearShader = clearShader;
            this.rasterShader = rasterShader;
            this.Resolution = resolution;
            Length = resolution * resolution * resolution;

            Initialize();
        }


        void Initialize()
        {
            TargetBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Length, Marshal.SizeOf<bool>());
            clearKernelIndex = clearShader.FindKernel("Clear");
            clearShader.GetKernelThreadGroupSizes(clearKernelIndex, out var x, out _, out _);
            clearThreadGroupSize = (int) x;

            renderKernelIndex = rasterShader.FindKernel("Render");
            rasterShader.SetInts("_Dimension", Resolution, Resolution, Resolution);
            rasterShader.GetKernelThreadGroupSizes(renderKernelIndex, out x, out _, out _);
            renderThreadGroupSize = (int) x;
        }

        public void Clear()
        {
            clearShader.SetBuffer(clearKernelIndex, "_Target", TargetBuffer);
            clearShader.Dispatch(clearKernelIndex, Length / clearThreadGroupSize, 1, 1);
        }

        public void Render()
        {
            rasterShader.SetBuffer(renderKernelIndex, "_Target", TargetBuffer);
            rasterShader.Dispatch(renderKernelIndex, Length / renderThreadGroupSize, 1, 1);
        }

        public void Dispose()
        {
            TargetBuffer.Dispose();
        }
    }
}
