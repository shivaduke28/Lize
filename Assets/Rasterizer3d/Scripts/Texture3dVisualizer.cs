using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rasterizer3d
{
    [DefaultExecutionOrder(1)]
    public sealed class Texture3dVisualizer : MonoBehaviour
    {
        [SerializeField] GameObject template;
        [SerializeField] Material material;
        [SerializeField] MeshRasterizer meshRasterizer;
        [SerializeField] Mesh mesh;
        [SerializeField] MeshFilter meshFilter;


        void Start()
        {
            var size = meshRasterizer.GetTextureSize;
            material.SetInt("_Size", size);
            material.SetTexture("_Tex3d", meshRasterizer.OutRenderTexture);

            mesh = CreateMesh(size);
            meshFilter.mesh = mesh;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct VertexLayout
        {
            public Vector3 Position;
            public Vector3 Normal;
            public UInt32 Id;
        }

        Mesh CreateCube()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[24]
            {
                // front
                new(-0.5f, -0.5f, -0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),
                // right
                new(0.5f, -0.5f, -0.5f),
                new(0.5f, -0.5f, 0.5f),
                new(0.5f, 0.5f, 0.5f),
                new(0.5f, 0.5f, -0.5f),
                // back
                new(0.5f, -0.5f, 0.5f),
                new(-0.5f, -0.5f, 0.5f),
                new(-0.5f, 0.5f, 0.5f),
                new(0.5f, 0.5f, 0.5f),
                // left
                new(-0.5f, -0.5f, 0.5f),
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, 0.5f),
                // top
                new(-0.5f, 0.5f, -0.5f),
                new(0.5f, 0.5f, -0.5f),
                new(0.5f, 0.5f, 0.5f),
                new(-0.5f, 0.5f, 0.5f),
                // bottom
                new(-0.5f, -0.5f, 0.5f),
                new(0.5f, -0.5f, 0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f, -0.5f),
            };

            mesh.normals = new Vector3[24]
            {
                new(0, 0, 1),
                new(0, 0, 1),
                new(0, 0, 1),
                new(0, 0, 1),
                new(1, 0, 0),
                new(1, 0, 0),
                new(1, 0, 0),
                new(1, 0, 0),
                new(0, 0, -1),
                new(0, 0, -1),
                new(0, 0, -1),
                new(0, 0, -1),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
            };

            mesh.triangles = new int[36]
            {
                0, 2, 1, 0, 3, 2,
                4, 6, 5, 4, 7, 6,
                8, 10, 9, 8, 11, 10,
                12, 14, 13, 12, 15, 14,
                16, 18, 17, 16, 19, 18,
                20, 22, 21, 20, 23, 22,
            };


            return mesh;
        }

        Mesh CreateMesh(int width)
        {
            var mesh = new Mesh();
            var texelCount = width * width * width;


            var cube = CreateCube();
            var texelVertices = cube.vertices;
            var texelVertexCount = cube.vertexCount;
            var texelNormals = cube.normals;
            var texelTriangles = cube.triangles;
            var texelTrisCount = texelTriangles.Length;

            var vertexCount = texelCount * texelVertexCount;
            var vertices = new NativeArray<VertexLayout>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var triangles = new int[texelCount * texelTrisCount];

            for (var i = 0; i < texelCount; i++)
            {
                var offset = i * texelVertexCount;
                for (var j = 0; j < texelVertexCount; j++)
                {
                    vertices[offset + j] = new VertexLayout
                    {
                        Position = texelVertices[j],
                        Normal = texelNormals[j],
                        Id = (uint) i
                    };
                }
                var trisOffset = i * texelTrisCount;
                for (var j = 0; j < texelTrisCount; j++)
                {
                    triangles[trisOffset + j] = texelTriangles[j] + offset;
                }
            }


            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.UInt32, 1),
            };

            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertexBufferParams(vertexCount, layout);
            mesh.SetVertexBufferData(vertices, 0, 0, vertexCount);
            mesh.triangles = triangles;
            return mesh;
        }
    }
}
