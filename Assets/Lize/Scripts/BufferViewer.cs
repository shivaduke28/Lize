using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Lize
{
    // Rasterizerの出力Bufferを3DTextureに書き込むやつ
    public sealed class BufferViewer : IDisposable
    {
        readonly RenderingContext context;
        readonly ComputeShader debugShader;

        RenderTexture renderTexture;

        int length;

        int copyKernel;
        int copyThreadGroupCount;

        public BufferViewer(ComputeShader debugShader, RenderingContext context)
        {
            this.context = context;
            this.debugShader = debugShader;
            Initialize();
        }

        void Initialize()
        {
            var resolution = context.Resolution;
            var param = new RenderTextureDescriptor
            {
                width = resolution,
                height = resolution,
                volumeDepth = resolution,
                dimension = TextureDimension.Tex3D,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat,
                depthStencilFormat = GraphicsFormat.None,
                msaaSamples = 1
            };
            renderTexture = new RenderTexture(param);
            renderTexture.Create();

            length = context.Count;

            copyKernel = debugShader.FindKernel("Copy");
            debugShader.GetKernelThreadGroupSizes(copyKernel, out var x, out _, out _);
            copyThreadGroupCount = (int) x;
        }

        public void Render()
        {
            debugShader.SetBuffer(copyKernel, "_Buffer", context.Buffer);
            debugShader.SetTexture(copyKernel, "_Texture", renderTexture);
            debugShader.Dispatch(copyKernel, length / copyThreadGroupCount, 1, 1);
        }

        public void Dispose()
        {
            GameObject.Destroy(renderTexture);
        }
    }
}
