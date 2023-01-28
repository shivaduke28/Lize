Shader "Test/Instancing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _3DTex ("3DTexture", 3D) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing // GPU instancing

            #include "UnityCG.cginc"
            #include <UnityInstancing.cginc>

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 追加
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler3D _3DTex;

            struct MyState
            {
                float3 Position;
                float3 Velocity; // not used
                float Speed; // not used
            };

            StructuredBuffer<MyState> myStateBufferResult;

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v); // 追加

                v2f o;
                float3 worldPos;
                #if defined(UNITY_INSTANCING_ENABLED)
                worldPos = mul(unity_ObjectToWorld, v.vertex) + myStateBufferResult[unity_InstanceID].Position;
                #else
                worldPos = mul(unity_ObjectToWorld, v.vertex);
                #endif
                o.vertex = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 c;
                #if defined(UNITY_INSTANCING_ENABLED)
                c = myStateBufferResult[unity_InstanceID].Velocity;
                return float4(1, 1, 1, 1) * 2.0;
                #else
                return float4(1, 1, 1, 1);
                #endif
            }
            ENDCG
        }
    }
}