using UnityEngine;

namespace Lize
{
    public static class MeshUtil
    {
        public static Mesh CreateCube()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[24]
            {
                // front
                new(-0.5f, -0.5f, -0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),
                // right
                new(0.5f, -0.5f, -0.5f),
                new(0.5f, -0.5f, 0.5f),
                new(0.5f, 0.5f, 0.5f),
                new(0.5f, 0.5f, -0.5f),
                // back
                new(0.5f, -0.5f, 0.5f),
                new(-0.5f, -0.5f, 0.5f),
                new(-0.5f, 0.5f, 0.5f),
                new(0.5f, 0.5f, 0.5f),
                // left
                new(-0.5f, -0.5f, 0.5f),
                new(-0.5f, -0.5f, -0.5f),
                new(-0.5f, 0.5f, -0.5f),
                new(-0.5f, 0.5f, 0.5f),
                // top
                new(-0.5f, 0.5f, -0.5f),
                new(0.5f, 0.5f, -0.5f),
                new(0.5f, 0.5f, 0.5f),
                new(-0.5f, 0.5f, 0.5f),
                // bottom
                new(-0.5f, -0.5f, 0.5f),
                new(0.5f, -0.5f, 0.5f),
                new(0.5f, -0.5f, -0.5f),
                new(-0.5f, -0.5f, -0.5f),
            };

            mesh.normals = new Vector3[24]
            {
                new(0, 0, -1),
                new(0, 0, -1),
                new(0, 0, -1),
                new(0, 0, -1),
                new(1, 0, 0),
                new(1, 0, 0),
                new(1, 0, 0),
                new(1, 0, 0),
                new(0, 0, 1),
                new(0, 0, 1),
                new(0, 0, 1),
                new(0, 0, 1),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(-1, 0, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
                new(0, -1, 0),
            };

            mesh.triangles = new int[36]
            {
                0, 2, 1, 0, 3, 2,
                4, 6, 5, 4, 7, 6,
                8, 10, 9, 8, 11, 10,
                12, 14, 13, 12, 15, 14,
                16, 18, 17, 16, 19, 18,
                20, 22, 21, 20, 23, 22,
            };


            return mesh;
        }
    }
}
