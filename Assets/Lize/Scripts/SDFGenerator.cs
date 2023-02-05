using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lize
{
    public sealed class SDFGenerator : IDisposable
    {
        readonly RenderingContext context;
        readonly BufferClearer bufferClearer;
        readonly ComputeShader sdfShader;

        int firstKernelIndex;
        int loopKernelIndex;
        int finalKernelIndex;

        readonly int firstThreadGroupSize;
        readonly int loopThreadGroupSize;
        readonly int finalThreadGroupSize;

        GraphicsBuffer buffer0;
        GraphicsBuffer buffer1;

        static class SDFKernel
        {
            public const string First = "First";
            public const string Loop = "Loop";
            public const string Final = "Final";

            public static readonly int RasterizerBuffer = Shader.PropertyToID("_RasterizerBuffer");
            public static readonly int TargetBuffer = Shader.PropertyToID("_TargetBuffer");
            public static readonly int SourceBuffer = Shader.PropertyToID("_SourceBuffer");
            public static readonly int SDFBuffer = Shader.PropertyToID("_SDFBuffer");
            public static readonly int Resolution = Shader.PropertyToID("_Resolution");
            public static readonly int Jump = Shader.PropertyToID("_Jump");
        }

        public SDFGenerator(BufferClearer bufferClearer, ComputeShader sdfShader, RenderingContext context)
        {
            this.context = context;
            this.bufferClearer = bufferClearer;
            this.sdfShader = sdfShader;
            firstKernelIndex = sdfShader.FindKernel(SDFKernel.First);
            loopKernelIndex = sdfShader.FindKernel(SDFKernel.Loop);
            finalKernelIndex = sdfShader.FindKernel(SDFKernel.Final);

            uint x;
            sdfShader.GetKernelThreadGroupSizes(firstKernelIndex, out x, out _, out _);
            firstThreadGroupSize = (int) x;
            sdfShader.GetKernelThreadGroupSizes(loopKernelIndex, out x, out _, out _);
            loopThreadGroupSize = (int) x;
            sdfShader.GetKernelThreadGroupSizes(finalKernelIndex, out x, out _, out _);
            finalThreadGroupSize = (int) x;

            buffer0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, context.Count, Marshal.SizeOf<uint>() * 4);
            buffer1 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, context.Count, Marshal.SizeOf<uint>() * 4);
        }

        public void Update()
        {
            // clear
            bufferClearer.Clear(context.SDFBuffer, context.Count);

            sdfShader.SetBuffer(firstKernelIndex, SDFKernel.RasterizerBuffer, context.Buffer);
            sdfShader.SetBuffer(firstKernelIndex, SDFKernel.TargetBuffer, buffer0);
            sdfShader.SetInt(SDFKernel.Resolution, context.Resolution);

            // resolution / 2で実行
            sdfShader.Dispatch(firstKernelIndex, Mathf.CeilToInt(context.Count / (float) firstThreadGroupSize), 1, 1);

            var exponent = (int) Mathf.Log(context.Resolution, 2);

            var isInputBuffer0 = true;
            for (var i = exponent - 2; i >= 0; i--)
            {
                var input = isInputBuffer0 ? buffer0 : buffer1;
                var output = isInputBuffer0 ? buffer1 : buffer0;
                sdfShader.SetBuffer(loopKernelIndex, SDFKernel.SourceBuffer, input);
                sdfShader.SetBuffer(loopKernelIndex, SDFKernel.TargetBuffer, output);
                sdfShader.SetInt(SDFKernel.Jump, (int) Mathf.Pow(2, i));
                sdfShader.Dispatch(loopKernelIndex, Mathf.CeilToInt(context.Count / (float) loopThreadGroupSize), 1, 1);
                isInputBuffer0 = !isInputBuffer0;
            }

            sdfShader.SetBuffer(finalKernelIndex, SDFKernel.SourceBuffer, isInputBuffer0 ? buffer0 : buffer1);
            sdfShader.SetBuffer(finalKernelIndex, SDFKernel.SDFBuffer, context.SDFBuffer);
            sdfShader.Dispatch(finalKernelIndex, Mathf.CeilToInt(context.Count / (float) finalThreadGroupSize), 1, 1);
        }

        public void Dispose()
        {
            buffer0?.Dispose();
            buffer1?.Dispose();
        }
    }
}
