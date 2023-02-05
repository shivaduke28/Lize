using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lize
{
    public sealed class RenderingContext : IDisposable
    {
        public int Resolution { get; }
        public int Count { get; }
        public GraphicsBuffer Buffer { get; }
        public GraphicsBuffer SDFBuffer { get; }

        public RenderingContext(int resolution)
        {
            Resolution = resolution;
            Count = resolution * resolution * resolution;
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Count, Marshal.SizeOf<bool>());
            SDFBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Count, Marshal.SizeOf<float>());
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
