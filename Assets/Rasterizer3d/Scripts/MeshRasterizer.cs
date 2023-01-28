using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rasterizer3d
{
    [DefaultExecutionOrder(0)]
    public sealed class MeshRasterizer : MonoBehaviour
    {
        [SerializeField] Mesh mesh;
        [SerializeField] ComputeShader computeShader;
        [SerializeField] RenderTexture outRenderTexture;
        [SerializeField] Transform target;

        GraphicsBuffer vertexBuffer;
        int clearKernelIndex;
        int mainKernelIndex;
        int vertexCount;

        const int Size = 32;

        public int GetTextureSize => Size;
        public Texture OutRenderTexture => outRenderTexture;

        struct Vertex
        {
            public Vector3 Position;
        }

        void Start()
        {
            Initialize();

            Dispatch();
        }

        void Update()
        {
            Dispatch();
        }

        void Initialize()
        {
            var param = new RenderTextureDescriptor
            {
                width = Size,
                height = Size,
                volumeDepth = Size,
                dimension = TextureDimension.Tex3D,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat,
                depthStencilFormat = GraphicsFormat.None,
                msaaSamples = 1
            };
            outRenderTexture = new RenderTexture(param);
            outRenderTexture.Create();

            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tris = mesh.triangles;
            vertexCount = vertices.Length;
            var nativeArray = new NativeArray<Vertex>(vertexCount,
                Allocator.Temp,
                NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < vertexCount; i++)
            {
                var v = vertices[i];
                nativeArray[i] = new Vertex
                {
                    Position = v,
                };
            }

            vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, nativeArray.Length, Marshal.SizeOf<Vertex>());
            vertexBuffer.SetData(nativeArray);
            nativeArray.Dispose();

            clearKernelIndex = computeShader.FindKernel("CSClear");
            mainKernelIndex = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(clearKernelIndex, "_Result", outRenderTexture);
            computeShader.SetBuffer(mainKernelIndex, "_VertexBuffer", vertexBuffer);
            computeShader.SetTexture(mainKernelIndex, "_Result", outRenderTexture);
            computeShader.SetInt("_Size", Size);
        }

        void Dispatch()
        {
            computeShader.SetMatrix("_ModelToWorldMatrix", target.localToWorldMatrix);
            computeShader.GetKernelThreadGroupSizes(clearKernelIndex, out var x, out var y, out var z);
            computeShader.Dispatch(clearKernelIndex, (int) (Size * Size * Size / x), 1, 1);
            computeShader.GetKernelThreadGroupSizes(mainKernelIndex, out var x2, out _, out _);
            computeShader.Dispatch(mainKernelIndex, (int) (vertexCount / x2), 1, 1);
        }
    }
}
