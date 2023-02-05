using System;
using UnityEngine;

namespace Lize
{
    public sealed class Installer : MonoBehaviour
    {
        [SerializeField] ComputeShader clearShader;
        [SerializeField] ComputeShader rasterShader;
        [SerializeField] ComputeShader debugShader;
        [SerializeField] bool isDebug;
        [SerializeField] int resolution = 32;

        Rasterizer rasterizer;
        BufferViewer bufferViewer;

        void Start()
        {
            rasterizer = new Rasterizer(clearShader, rasterShader, resolution);
            bufferViewer = new BufferViewer(rasterizer, debugShader);
        }

        void Update()
        {
            rasterizer.Clear();
            rasterizer.Render();
            bufferViewer.Render();
        }
    }
}
