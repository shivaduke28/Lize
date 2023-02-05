using UnityEngine;

namespace Lize
{
    public sealed class Installer : MonoBehaviour
    {
        [SerializeField] ComputeShader clearShader;
        [SerializeField] ComputeShader vertexShader;
        [SerializeField] ComputeShader triangleShader;
        [SerializeField] ComputeShader debugShader;
        [SerializeField] int resolution = 32;
        [Space] [SerializeField] MeshFilter meshFilter;
        [SerializeField] Camera rasterizerCamera;

        [SerializeField] RasterizerBufferRenderer bufferRenderer;

        Rasterizer rasterizer;
        BufferViewer bufferViewer;
        RasterizerContext context;

        void Start()
        {
            context = new RasterizerContext(resolution);
            rasterizer = new Rasterizer(clearShader, vertexShader, triangleShader, context);
            bufferViewer = new BufferViewer(debugShader, context);
            bufferRenderer.Construct(context);
        }

        void Update()
        {
            rasterizer.Clear();
            rasterizer.Render(rasterizerCamera, meshFilter);
            //bufferViewer.Render();
        }

        void OnDestroy()
        {
            rasterizer.Dispose();
            bufferViewer.Dispose();
            context.Dispose();
        }
    }
}
