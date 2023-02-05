using UnityEngine;

namespace Lize
{
    public sealed class RenderingManager : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField] MeshFilter[] meshFilters;

        Rasterizer rasterizer;
        RenderingContext renderingContext;
        SDFGenerator sdfGenerator;

        public void Construct(Rasterizer rasterizer, RenderingContext renderingContext, SDFGenerator sdfGenerator)
        {
            this.renderingContext = renderingContext;
            this.rasterizer = rasterizer;
            this.sdfGenerator = sdfGenerator;
        }

        void LateUpdate()
        {
            rasterizer.Clear();
            foreach (var meshFilter in meshFilters)
            {
                rasterizer.Render(targetCamera, meshFilter);
            }
            sdfGenerator.Update();
        }
    }
}
