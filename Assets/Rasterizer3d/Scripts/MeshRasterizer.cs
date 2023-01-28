﻿using System;
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
        [SerializeField] ComputeShader clearShader;
        [SerializeField] ComputeShader rasterizerShader;
        [SerializeField] RenderTexture outRenderTexture; // 3d target texture
        [SerializeField] Transform meshTransform;
        [SerializeField] Camera camera;

        Transform cameraTransform;
        GraphicsBuffer vertexBuffer;
        GraphicsBuffer vertexResultBuffer;
        GraphicsBuffer triangleBuffer;

        int clearKernelIndex;
        int vertexKernelIndex;
        int triangleKernelIndex;

        int vertexCount;
        int trisCount;
        const int TexelSize = 32;
        public int GetTextureSize => TexelSize;
        public Texture OutRenderTexture => outRenderTexture;

        struct Vertex
        {
            public Vector3 Position;
        }

        void Start()
        {
            cameraTransform = camera.transform;
            Initialize();
        }

        void Update()
        {
            Clear();
            Render();
        }

        void Initialize()
        {
            var param = new RenderTextureDescriptor
            {
                width = TexelSize,
                height = TexelSize,
                volumeDepth = TexelSize,
                dimension = TextureDimension.Tex3D,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat,
                depthStencilFormat = GraphicsFormat.None,
                msaaSamples = 1
            };
            outRenderTexture = new RenderTexture(param);
            outRenderTexture.Create();

            var vertices = mesh.vertices;
            vertexCount = vertices.Length;

            vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf<float>() * 3);
            vertexResultBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf<float>() * 3);
            vertexBuffer.SetData(vertices);

            var tris = mesh.triangles;
            trisCount = tris.Length;

            triangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, trisCount, Marshal.SizeOf<uint>());
            triangleBuffer.SetData(tris);

            clearKernelIndex = clearShader.FindKernel("CSMain");
            clearShader.SetInt("_TexelSize", TexelSize);
            clearShader.SetTexture(clearKernelIndex, "_TargetTex", outRenderTexture);

            vertexKernelIndex = rasterizerShader.FindKernel("CSVertex");
            triangleKernelIndex = rasterizerShader.FindKernel("CSTriangle");

            rasterizerShader.SetBuffer(vertexKernelIndex, "_VertexBuffer", vertexBuffer);
            rasterizerShader.SetBuffer(vertexKernelIndex, "_VertexResultBuffer", vertexResultBuffer);
            rasterizerShader.SetBuffer(triangleKernelIndex, "_VertexResultBuffer", vertexResultBuffer);
            rasterizerShader.SetBuffer(triangleKernelIndex, "_TriangleBuffer", triangleBuffer);
            rasterizerShader.SetTexture(triangleKernelIndex, "_TargetTex", outRenderTexture);
            rasterizerShader.SetInt("_TexelSize", TexelSize);
        }

        void Clear()
        {
            clearShader.GetKernelThreadGroupSizes(clearKernelIndex, out var x, out var y, out var z);
            clearShader.Dispatch(clearKernelIndex, (int) (TexelSize * TexelSize * TexelSize / x), 1, 1);
        }

        void Render()
        {
            rasterizerShader.SetMatrix("_ModelToWorldMatrix", meshTransform.localToWorldMatrix);
            rasterizerShader.SetMatrix("_WorldToCameraMatrix", cameraTransform.worldToLocalMatrix);
            var orthSize = camera.orthographicSize * 2;
            rasterizerShader.SetVector("_CameraData", new Vector4(orthSize, orthSize, camera.nearClipPlane, camera.farClipPlane));
            rasterizerShader.GetKernelThreadGroupSizes(vertexKernelIndex, out var x, out _, out _);
            rasterizerShader.Dispatch(vertexKernelIndex, (int) (vertexCount / x) + 1, 1, 1);

            rasterizerShader.GetKernelThreadGroupSizes(triangleKernelIndex, out var x2, out _, out _);
            rasterizerShader.Dispatch(triangleKernelIndex, (int) ((trisCount / 3) / x2) + 1, 1, 1);
        }
    }
}
