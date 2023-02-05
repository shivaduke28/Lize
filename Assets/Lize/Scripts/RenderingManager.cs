using UnityEngine;

namespace Lize
{
    public sealed class RenderingManager : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField] MeshFilter[] meshFilters;

        Rasterizer rasterizer;
        RenderingContext renderingContext;

        public void Construct(Rasterizer rasterizer, RenderingContext renderingContext)
        {
            this.renderingContext = renderingContext;
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
