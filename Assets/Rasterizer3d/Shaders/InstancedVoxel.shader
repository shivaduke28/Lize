Shader "Unlit/InstancedVoxel"
{
    Properties
    {
        _Tex3d ("Tex3d", 3D) = "" {}
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
            #pragma multi_compile_instancing // GPU instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD1;
            };

            sampler3D _Tex3d;
            uint _TexelSize;

            float3 id2uv(uint id)
            {
                float size = _TexelSize;
                float z = floor(id / (size * size));
                uint i = id - z * size * size;
                float y = floor(i / size);
                float x = floor(i - y * size);
                float3 uv = (float3(x, y, z) + float3(0.5, 0.5, 0.5)) / size;
                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 uv = id2uv(v.instanceID);
                float4 col = tex3Dlod(_Tex3d, float4(uv, 0));
                float3 positionOS = v.vertex * 0.1 * col.a;

                float3 worldPos = mul(unity_ObjectToWorld, float4(positionOS, 1));
                worldPos += (uv - 0.5) * 4;
                o.vertex = UnityWorldToClipPos(worldPos);

                o.uv = v.uv;
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 col = 1;
                float3 n = normalize(i.normalWS);
                col *= max(0, dot(n, float3(0.2, 1, 0.1))) + 0.2;
                return float4(col, 1);
            }
            ENDCG
        }
    }
}