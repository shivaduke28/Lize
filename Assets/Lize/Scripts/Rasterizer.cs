using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lize
{
    public sealed class Rasterizer : IDisposable
    {
        readonly ComputeShader clearShader;
        readonly ComputeShader vertexShader;
        readonly ComputeShader triangleShader;
        readonly RenderingContext context;

        int clearKernelIndex;
        int clearThreadGroupSize;

        int vertexKernelIndex;
        int vertexThreadGroupSize;

        int triangleKernelIndex;
        int triangleThreadGroupSize;

        GraphicsBuffer vertexCSBuffer;

        public Rasterizer(ComputeShader clearShader, ComputeShader vertexShader, ComputeShader triangleShader, RenderingContext context)
        {
            this.clearShader = clearShader;
            this.vertexShader = vertexShader;
            this.triangleShader = triangleShader;
            this.context = context;

            Initialize();
        }


        void Initialize()
        {
            clearKernelIndex = clearShader.FindKernel("Clear");
            clearShader.GetKernelThreadGroupSizes(clearKernelIndex, out var x, out _, out _);
            clearThreadGroupSize = (int) x;

            vertexKernelIndex = vertexShader.FindKernel("Vertex");
            vertexShader.GetKernelThreadGroupSizes(vertexKernelIndex, out x, out _, out _);
            vertexThreadGroupSize = (int) x;

            triangleKernelIndex = triangleShader.FindKernel("Raster");
            triangleShader.GetKernelThreadGroupSizes(triangleKernelIndex, out x, out _, out _);
            triangleThreadGroupSize = (int) x;
        }

        public void Clear()
        {
            clearShader.SetBuffer(clearKernelIndex, "_Target", context.Buffer);
            clearShader.Dispatch(clearKernelIndex, Mathf.CeilToInt(context.Count / (float) clearThreadGroupSize), 1, 1);
        }

        public void Render(Camera camera, MeshFilter meshFilter)
        {
            var modelToWorld = meshFilter.transform.localToWorldMatrix;
            var worldToCamera = camera.transform.worldToLocalMatrix;
            var mesh = meshFilter.sharedMesh;
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

            var positionStream = mesh.GetVertexAttributeStream(VertexAttribute.Position);
            var vertexBuffer = mesh.GetVertexBuffer(positionStream);
            var stride = mesh.GetVertexBufferStride(positionStream);
            var offset = mesh.GetVertexAttributeOffset(VertexAttribute.Position);
            var vertexCount = mesh.vertexCount;

            // TODO: meshごとに再利用するかなんかする?
            vertexCSBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertexCount, Marshal.SizeOf<float>() * 3);

            vertexShader.SetBuffer(vertexKernelIndex, "_VertexOSBuffer", vertexBuffer);
            vertexShader.SetBuffer(vertexKernelIndex, "_VertexCSBuffer", vertexCSBuffer);
            // vertexShader.SetBuffer(vertexKernelIndex, "_Target", TargetBuffer); // for debug

            vertexShader.SetInt("_Stride", stride);
            vertexShader.SetInt("_Offset", offset);
            vertexShader.SetInt("_VertexCount", vertexCount);

            vertexShader.SetMatrix("_ModelToCameraMatrix", worldToCamera * modelToWorld);
            var size = camera.orthographicSize;
            vertexShader.SetVector("_CameraData", new Vector4(size, size, camera.nearClipPlane, camera.farClipPlane));
            vertexShader.Dispatch(vertexKernelIndex, Mathf.CeilToInt(vertexCount / (float) vertexThreadGroupSize), 1, 1);

            vertexBuffer.Release();

            mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
            var indexBuffer = mesh.GetIndexBuffer();
            triangleShader.SetBuffer(triangleKernelIndex, "_IndexBuffer", indexBuffer);
            triangleShader.SetBuffer(triangleKernelIndex, "_VertexCSBuffer", vertexCSBuffer);
            triangleShader.SetBuffer(triangleKernelIndex, "_Target", context.Buffer);
            triangleShader.SetInt("_TexelSize", context.Resolution);
            triangleShader.SetBool("_Index32Bit", mesh.indexFormat == IndexFormat.UInt32);
            var trisCount = (int) mesh.GetIndexCount(0) / 3;
            triangleShader.SetInt("_TriangleCount", trisCount);
            triangleShader.Dispatch(triangleKernelIndex, Mathf.CeilToInt(trisCount / (float) triangleThreadGroupSize), 1, 1);

            vertexCSBuffer.Release();
            indexBuffer.Release();
        }

        public void Dispose()
        {
            vertexCSBuffer.Dispose();
        }
    }
}
