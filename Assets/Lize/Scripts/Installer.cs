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

        Rasterizer rasterizer;
        BufferViewer bufferViewer;

        void Start()
        {
            rasterizer = new Rasterizer(clearShader, vertexShader, triangleShader, resolution);
            bufferViewer = new BufferViewer(rasterizer, debugShader);
        }

        void Update()
        {
            rasterizer.Clear();
            rasterizer.Render(rasterizerCamera, meshFilter);
            bufferViewer.Render();
        }

        void OnDestroy()
        {
            rasterizer.Dispose();
            bufferViewer.Dispose();
        }
    }
}
