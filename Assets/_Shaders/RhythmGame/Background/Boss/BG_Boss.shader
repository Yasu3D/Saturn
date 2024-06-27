Shader "SaturnGame/RhythmGame/Backgrounds/Boss"
{
    Properties
    {
        [NoScaleOffset] _CheckerMap ("Checker Map", 2D) = "white" {}
        [NoScaleOffset] _GlowMap ("Glow Map", 2D) = "white" {}
        
        _DepthRed ("Depth Redness", Float) = 100
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

            sampler2D _CheckerMap;
            sampler2D _GlowMap;
            
            float4 _CheckerMap_ST;
            float4 _GlowMap_ST;

            float _DepthRed;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert(appdata v)
            {
                v2f output;
                
                output.uv = v.uv;
                output.clip = UnityObjectToClipPos(v.vertex);
                output.color = v.color;
                
                return output;
            }

            float4 GetGlowMap(float4 clip)
            {
                return tex2D(_GlowMap, float2(clip.x, clip.y) * 0.00097) * float4(0.9, 0.9, 0.9, 1);
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 checkers = tex2D(_CheckerMap, i.uv);
                float3 pink = checkers.x * float3(-2, -1.16741, -1.84059) + float3(3, 1.17783, 1.85101);
                float3 red = checkers.x * float3(0.77051, 0.01971, 0.063) + float3(0.03646, 0.001, 0.002);
                float depth = min(1, exp2(log2(max(0, i.clip.w / _DepthRed)) * 4));
                float depth2 = 1 - min(1, exp2(log2(max(0, i.clip.w / 50)) * 4));
                
                float3 mixed = lerp(pink, red, depth);
                mixed = lerp(mixed, mixed * 1.3 + float3(0.39583, 0.00825, 0.08767), depth2);
                mixed = (GetGlowMap(i.clip) + mixed) * i.color.xyz;
                
                return float4(mixed, 1);
            }
            ENDCG
        }
    }
}