using System;
using UnityEngine;

namespace Lize
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RasterizerBufferRenderer : MonoBehaviour
    {
        [SerializeField] BoxCollider boxCollider;
        [SerializeField] Material material;
        Mesh mesh;
        ComputeBuffer bufferWithArgs;

        RenderingContext context;
        bool isInitialized;

        public void Construct(RenderingContext context)
        {
            this.context = context;
            material.SetBuffer("_Buffer", context.Buffer);
            material.SetVector("_Dimension", new Vector4(context.Resolution, context.Resolution, context.Resolution));
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
