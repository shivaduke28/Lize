using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Lize
{
    public sealed class RasterizerContext : IDisposable
    {
        public int Resolution { get; }
        public int Count { get; }
        public GraphicsBuffer Buffer { get; }

        public RasterizerContext(int resolution)
        {
            Resolution = resolution;
            Count = resolution * resolution * resolution;
            Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Count, Marshal.SizeOf<bool>());
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
