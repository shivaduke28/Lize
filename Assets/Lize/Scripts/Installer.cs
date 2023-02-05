using UnityEngine;

namespace Lize
{
    public sealed class Installer : MonoBehaviour
    {
        [SerializeField] ComputeShader clearShader;
        [SerializeField] ComputeShader vertexShader;
        [SerializeField] ComputeShader triangleShader;
        [SerializeField] int resolution = 32;

        [SerializeField] RenderingManager renderingManager;
        [SerializeField] RasterizerBufferRenderer bufferRenderer;

        Rasterizer rasterizer;
        BufferViewer bufferViewer;
        RenderingContext context;

        void Start()
        {
            context = new RenderingContext(resolution);
            rasterizer = new Rasterizer(clearShader, vertexShader, triangleShader, context);
            renderingManager.Construct(rasterizer, context);
            // bufferViewer = new BufferViewer(debugShader, context);
            bufferRenderer.Construct(context);
        }

        void OnDestroy()
        {
            rasterizer.Dispose();
            // bufferViewer.Dispose();
            context.Dispose();
        }
    }
}
