using UnityEngine;

namespace Rasterizer3d
{
    [DefaultExecutionOrder(10)]
    public sealed class VoxelView : MonoBehaviour
    {
        [SerializeField] MeshRasterizer rasterizer;
        [SerializeField] Material material;
        [SerializeField] Bounds bounds;
        Mesh mesh;

        ComputeBuffer bufferWithArgs;
        int gridPointCount;

        void Start()
        {
            mesh = MeshUtil.CreateCube();
            var texelSize = rasterizer.GetTextureSize;
            gridPointCount = texelSize * texelSize * texelSize;
            material.SetInt("_TexelSize", texelSize);
            material.SetTexture("_Tex3d", rasterizer.OutRenderTexture);
            CreateArgsBuffer();
        }

        void Update()
        {
            bounds.center = transform.position;
            Render();
        }

        void CreateArgsBuffer()
        {
            // count, stride, type
            bufferWithArgs = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            // number of sub meshes, draw count, ?, ?, ?
            bufferWithArgs.SetData(new uint[] { mesh.GetIndexCount(0), (uint) gridPointCount, 0, 0, 0 });
        }

        void Render()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, bufferWithArgs);
        }
    }
}
