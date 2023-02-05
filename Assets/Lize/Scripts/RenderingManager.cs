using UnityEngine;

namespace Lize
{
    public sealed class RenderingManager : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField] MeshFilter[] meshFilters;

        Rasterizer rasterizer;
        RasterizerContext rasterizerContext;

        public void Construct(Rasterizer rasterizer, RasterizerContext rasterizerContext)
        {
            this.rasterizerContext = rasterizerContext;
            this.rasterizer = rasterizer;
        }

        void LateUpdate()
        {
            rasterizer.Clear();
            foreach (var meshFilter in meshFilters)
            {
                rasterizer.Render(targetCamera, meshFilter);
            }
        }
    }
}
