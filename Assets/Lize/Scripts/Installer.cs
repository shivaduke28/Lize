using UnityEngine;

namespace Lize
{
    public sealed class Installer : MonoBehaviour
    {
        [SerializeField] ComputeShader clearShader;
        [SerializeField] ComputeShader vertexShader;
        [SerializeField] ComputeShader triangleShader;
        [SerializeField] ComputeShader sdfShader;
        [SerializeField] int resolution = 32;

        [SerializeField] RenderingManager renderingManager;
        [SerializeField] RasterizerBufferRenderer bufferRenderer;

        Rasterizer rasterizer;
        BufferViewer bufferViewer;
        RenderingContext context;
        SDFGenerator sdfGenerator;

        void Start()
        {
            context = new RenderingContext(resolution);
            rasterizer = new Rasterizer(clearShader, vertexShader, triangleShader, context);
            var bufferClearer = new BufferClearer(clearShader);
            sdfGenerator = new SDFGenerator(bufferClearer, sdfShader, context);
            renderingManager.Construct(rasterizer, context, sdfGenerator);
            // bufferViewer = new BufferViewer(debugShader, context);
            bufferRenderer.Construct(context);
        }

        void OnDestroy()
        {
            rasterizer.Dispose();
            sdfGenerator.Dispose();
            // bufferViewer.Dispose();
            context.Dispose();
        }
    }
}
