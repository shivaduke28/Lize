Shader "Lize/RasterizerBuffer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            uint3 _Dimension;

            StructuredBuffer<bool> _Buffer;

            float3 id2pos(uint id)
            {
                float z = floor(id / (_Dimension.x * _Dimension.y));
                uint i = id - z * _Dimension.x * _Dimension.y;
                float y = floor(i / _Dimension.x);
                float x = floor(i - y * _Dimension.x);
                return float3(x, y, z);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 positionOS = v.vertex;
                uint id = v.instanceID;
                bool value = _Buffer[id];
                positionOS *= (uint)value;
                positionOS += id2pos(id);

                o.vertex = UnityObjectToClipPos(float4(positionOS, 1));
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = 1;
                float3 n = normalize(i.normalWS);
                col *= max(0, dot(n, float3(1, 1, 1)));
                col.rgb *= n;
                return col;
            }
            ENDCG
        }
    }
}