using System;
using UnityEngine;

namespace Lize
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RasterizerBufferRenderer : MonoBehaviour
    {
        [SerializeField] BoxCollider boxCollider;
        [SerializeField] Material material;
        [SerializeField] float scale = 1;
        Mesh mesh;
        ComputeBuffer bufferWithArgs;

        RenderingContext context;
        bool isInitialized;

        static class ShaderProperties
        {
            public static readonly int Buffer = Shader.PropertyToID("_Buffer");
            public static readonly int SDFBuffer = Shader.PropertyToID("_SDFBuffer");
            public static readonly int Dimension = Shader.PropertyToID("_Dimension");
            public static readonly int DimensionInv = Shader.PropertyToID("_DimensionInv");
            public static readonly int Bounds = Shader.PropertyToID("_Bounds");
            public static readonly int Scale = Shader.PropertyToID("_Scale");
        }

        public void Construct(RenderingContext context)
        {
            this.context = context;
            material.SetBuffer(ShaderProperties.Buffer, context.Buffer);
            material.SetBuffer(ShaderProperties.SDFBuffer, context.SDFBuffer);
            material.SetVector(ShaderProperties.Dimension, new Vector4(context.Resolution, context.Resolution, context.Resolution));
            material.SetVector(ShaderProperties.DimensionInv, new Vector4(1f / context.Resolution, 1f / context.Resolution, 1f / context.Resolution));
            mesh = MeshUtil.CreateCube();
            bufferWithArgs?.Dispose();

            // count, stride (8byte * 5), type
            bufferWithArgs = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

            // number of sub meshes, draw count, ?, ?, ?
            bufferWithArgs.SetData(new uint[] { mesh.GetIndexCount(0), (uint) context.Count, 0, 0, 0 });

            isInitialized = true;
        }

        void LateUpdate()
        {
            var bounds = boxCollider.bounds;
            material.SetVector(ShaderProperties.Bounds, bounds.size);
            material.SetFloat(ShaderProperties.Scale, scale);
            Render();
        }

        void Render()
        {
            if (!isInitialized) return;
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, boxCollider.bounds, bufferWithArgs);
        }

        void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
            bufferWithArgs?.Dispose();
        }
    }
}
