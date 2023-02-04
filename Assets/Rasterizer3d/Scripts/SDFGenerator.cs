using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rasterizer3d
{
    [DefaultExecutionOrder(10)]
    public sealed class SDFGenerator : MonoBehaviour
    {
        [SerializeField] MeshRasterizer rasterizer;
        [SerializeField] ComputeShader jfaShader;
        Texture input;
        Texture output;
        Material material;

        RenderTextureDescriptor descriptor;

        int kernelIndexFirst;
        int kernelIndexSecond;
        int kernelIndexFinal;


        void Start()
        {
            input = rasterizer.OutRenderTexture;
            var textureSize = rasterizer.TextureSize;
            output = CreateTexture(textureSize);
            descriptor = CreateDescriptor(textureSize);

            kernelIndexFirst = jfaShader.FindKernel("CSFirst");
            kernelIndexSecond = jfaShader.FindKernel("CSSecond");
            kernelIndexFinal = jfaShader.FindKernel("CSFinal");
        }

        void Update()
        {
            Render();
        }

        void Render()
        {
            var textureSize = rasterizer.TextureSize;
            var rt0 = RenderTexture.GetTemporary(descriptor);
            var rt1 = RenderTexture.GetTemporary(descriptor);
            var exponent = Mathf.Log(textureSize, 2) - 1; // 64 -> 8

            var count = textureSize * textureSize * textureSize;

            jfaShader.SetTexture(kernelIndexFirst, "_Input", input);
            jfaShader.SetTexture(kernelIndexFirst, "_Output", rt0);
            jfaShader.SetInt("_JumpSize", (int) Mathf.Pow(exponent, 2.0f));
            jfaShader.GetKernelThreadGroupSizes(kernelIndexFirst, out var x, out _, out _);
            jfaShader.Dispatch(kernelIndexFirst, (int) (count / x), 1, 1);

            var inputIsRT0 = true;
            for (var i = exponent - 1; i >= 1; i--)
            {
                var currentInput = inputIsRT0 ? rt0 : rt1;
                var currentOutput = inputIsRT0 ? rt1 : rt0;

                var jumpSize = (int) Mathf.Pow(2, i);
                jfaShader.SetInt("_JumpSize", jumpSize);
                jfaShader.SetTexture(kernelIndexSecond, "_Input", currentInput);
                jfaShader.SetTexture(kernelIndexSecond, "_Output", currentOutput);
                jfaShader.Dispatch(kernelIndexSecond, (int) (count / x), 1, 1);
                inputIsRT0 = !inputIsRT0;
            }

            jfaShader.SetTexture(kernelIndexFinal, "_Input", inputIsRT0 ? rt0 : rt1);
            jfaShader.SetTexture(kernelIndexFinal, "_Output", output);
            jfaShader.SetInt("_JumpSize", 1);
            jfaShader.Dispatch(kernelIndexFinal, (int) (count / x), 1, 1);
        }


        static RenderTextureDescriptor CreateDescriptor(int texelSize)
        {
            return new RenderTextureDescriptor
            {
                width = texelSize,
                height = texelSize,
                volumeDepth = texelSize,
                dimension = TextureDimension.Tex3D,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat,
                depthStencilFormat = GraphicsFormat.None,
                msaaSamples = 1
            };
        }

        static RenderTexture CreateTexture(int texelSize)
        {
            var param = CreateDescriptor(texelSize);
            var rt = new RenderTexture(param);
            rt.Create();
            return rt;
        }
    }
}
