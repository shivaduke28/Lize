Shader "Lize/RasterizerBuffer"
{
    Properties {}
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
            #include "Turbo.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 color : TEXCOORD2;
            };

            float3 _Bounds;
            float _Scale;

            float3 _Dimension;
            float3 _DimensionInv;
            float4x4 _ModelToWorldMatrix;

            // StructuredBuffer<bool> _Buffer; // not used
            StructuredBuffer<float> _SDFBuffer;

            float3 id2pos(uint id)
            {
                float z = floor(id * _DimensionInv.x * _DimensionInv.y);
                uint i = id - z * _Dimension.x * _Dimension.y;
                float y = floor(i * _DimensionInv.x);
                float x = floor(i - y * _Dimension.x);
                return float3(x, y, z) - (_Dimension - 1) * 0.5;
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 positionOS = v.vertex;
                float3 normalWS = UnityObjectToWorldNormal(v.normal);
                uint id = v.instanceID;
                float dist = _SDFBuffer[id];
                float3 scale = _Bounds * _DimensionInv;
                float heat = 1;
                if (dist <= 0)
                {
                    o.color = max(0.1, dot(normalWS, float3(0.5, 1, -1)));
                }
                else
                {
                    heat = 1 - dist / length(_Dimension) * 2.2;
                    o.color = TurboColormap(heat);
                    positionOS.y += sin(_Time.y * heat * heat * 2 + id) * 1;
                }
                positionOS *= _Scale;
                positionOS *= dist <= 0 ? 1 : smoothstep(0.1, 1.2, heat) * 0.15 + 0.04;
                positionOS += id2pos(id);
                positionOS *= scale;
                o.vertex = UnityObjectToClipPos(float4(positionOS, 1));
                o.normalWS = normalWS;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(i.color, 1);
            }
            ENDCG
        }
    }
}