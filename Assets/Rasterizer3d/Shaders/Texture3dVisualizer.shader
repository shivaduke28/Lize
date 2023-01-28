Shader "Unlit/Texture3dVisualizer"
{
    Properties
    {
        [NoScaleOffset] _Tex3d ("Texture", 3D) = "white" {}
        _Size ("Texture Size", int) = 32
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

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                uint id : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler3D _Tex3d;
            float4 _MainTex_ST;
            uint _Size;

            v2f vert(appdata v)
            {
                v2f o;
                uint id = v.id;

                float size = _Size;
                uint index = id;
                float z = floor(index / (size * size));
                uint i = index - z * size * size;
                float y = floor(i / size);
                float x = i - y * size;
                float3 uv = float3(x, y, z) / size;
                float4 col = tex3Dlod(_Tex3d, float4(uv, 0));

                float3 v2 = v.vertex.xyz;
                v2 *= col.a * 0.5;
                float3 worldPos = mul(unity_ObjectToWorld, float4(v2, 1));
                worldPos += (uv - float3(0.5, 0.5, 0.5)) * 20;
                o.vertex = UnityWorldToClipPos(worldPos);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 diffuse = float3(1, 1, 1);
                float3 n = normalize(i.normalWS);
                float3 l = normalize(float3(0.5, 1, 0.3));
                float3 col = max(0, dot(n, l)) * diffuse;
                float3 l2 = float3(-0.5, -1, -0.3);
                col += max(0, dot(n, l2)) * float3(0, 0.2, 0.2) * diffuse;
                col += float3(0.1, 0, 0.05) * diffuse;
                return float4(col, 1);
            }
            ENDCG
        }
    }
}