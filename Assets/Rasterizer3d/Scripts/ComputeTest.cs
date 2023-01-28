using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rasterizer3d
{
    public sealed class ComputeTest : MonoBehaviour
    {
        [SerializeField] ComputeShader computeShader;
        [SerializeField] int count = 100;
        [SerializeField] float extent = 1f;
        [SerializeField] GameObject template;
        [SerializeField] bool power;
        GraphicsBuffer buffer;
        int kernelIndex;


        public struct MyState
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Speed;
        }

        void OnEnable()
        {
            buffer = CreateBuffer(count, extent, template);
            kernelIndex = computeShader.FindKernel("CSMain");
            computeShader.SetBuffer(kernelIndex, "myStateBuffer", buffer);
            Shader.SetGlobalBuffer("myStateBufferResult", buffer);
        }

        void Update()
        {
            UpdateShader();
        }

        [ContextMenu("UpdateShader")]
        void UpdateShader()
        {
            computeShader.SetFloat("deltaTime", Time.deltaTime);
            computeShader.SetVector("target", transform.position);
            computeShader.SetBool("power", power);
            computeShader.GetKernelThreadGroupSizes(kernelIndex, out var x, out var y, out var z);
            computeShader.Dispatch(kernelIndex, (int) (count / x), 1, 1);
        }

        static GraphicsBuffer CreateBuffer(int count, float extent, GameObject template)
        {
            var array = new NativeArray<MyState>(count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < array.Length; i++)
            {
                var pos = new Vector3(
                    Random.Range(-extent, extent),
                    Random.Range(-extent, extent),
                    Random.Range(-extent, extent)
                );
                array[i] = new MyState
                {
                    Position = pos,
                    Velocity = - pos.normalized,
                    Speed = Random.Range(0.9f, 1.1f),
                };

                var go = GameObject.Instantiate(template, Vector3.zero, Quaternion.identity);
                go.SetActive(true);
            }

            // StructuredBufferなので、Structured, 長さとサイズを渡す
            var buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, array.Length, Marshal.SizeOf<MyState>());
            buffer.SetData(array);
            array.Dispose();
            return buffer;
        }
    }
}
