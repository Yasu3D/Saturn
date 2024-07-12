Shader "SaturnGame/RhythmGame/Notes/Note"
{
    Properties
    {
        [NoScaleOffset] _NoteMap1 ("Note Map 1", 2D) = "white" {}
        [NoScaleOffset] _NoteMap2 ("Note Map 2", 2D) = "white" {}
        _NoteColor ("Note Color", Color) = (1,1,1,1)
        
        [Toggle] _IsChain ("Is Chain", Integer) = 0
        [Toggle] _IsBonus ("Is Bonus", Integer) = 0
        [Toggle] _IsSync ("Is Sync", Integer) = 0
        
        [IntRange] _NoteSize ("Note Size", Range(1, 5)) = 3
        _ZOffset ("Z Offset", Integer) = 0
        
        
        _DebugNoteSize ("Debug NoteSize", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoteMap1;
            sampler2D _NoteMap2;
            float4 _NoteMap1_ST;
            float4 _NoteMap2_ST;
            float4 _NoteColor;

            int _NoteSize;
            int _ZOffset;
            
            bool _IsChain;
            bool _IsBonus;
            bool _IsSync;

            float _DebugNoteSize;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f output;
                
                output.uv0 = v.uv0;
                output.uv1 = v.uv1;
                output.clip = UnityObjectToClipPos(v.vertex);
                
                return output;
            }

            float4 frag(v2f i) : SV_Target
            {
                const float3 color = pow(_NoteColor.xyz, 1 / 2.2);
                const float3 colorSquared = color * color * 0.64;
                const float3 colorInverted = -color + 0.65;
                
                const float sizes[] = {0.85, 0.8, 0.5, 0.3, 0};
                float2 uvA = i.uv0 + float2(0.4 * sizes[_NoteSize - 1], 0);
                float2 uvB = i.uv0 - float2(0.4 * sizes[_NoteSize - 1], 0);
                float2 uvC = (i.uv0 + float2(-sizes[_NoteSize - 1] * 0.4, 0)) / float2(1 - sizes[_NoteSize - 1] * 0.8, 1);

                // Sample Textures
                float4 noteMap1A = tex2D(_NoteMap1, uvA);
                float4 noteMap1B = tex2D(_NoteMap1, uvB);
                
                float4 noteMap2A = tex2D(_NoteMap2, uvA);
                float4 noteMap2B = tex2D(_NoteMap2, uvB);
                float4 noteMap2C = tex2D(_NoteMap2, uvC);

                // Calculate some masks
                float2 uvMask = i.uv0.y > float2(0.601, 0.4);
                float sideMask = saturate((i.uv0.x * 2 - 1) * -12.5);
                float noteMask = lerp(noteMap2A.x, noteMap2B.x, sideMask);
                float shapeMask = noteMask * uvMask.y;

                float chainMask = (1 - uvMask.y) * noteMask * noteMap1A.w;

                // Clip alpha
                float syncAlpha = (uvMask.x - 1) * noteMask + noteMask;
                float finalAlpha = _IsSync * syncAlpha + (1 - uvMask.x) * noteMask;
                clip(finalAlpha - 0.3);
                
                // Outer Side
                float3 result = lerp(color, colorSquared, noteMap1A.x);
                result = lerp(result, result * colorInverted - colorInverted + 1, noteMap1A.y);
                result += noteMap1A.z * 0.08;
                result = lerp(result, noteMap1A.xyz, shapeMask);

                // Inner Side
                float3 result2 = lerp(color, colorSquared, noteMap1B.x);
                result2 = lerp(result2, result2 * colorInverted - colorInverted + 1, noteMap1B.y);
                result2 += noteMap1B.z * 0.08;
                result2 = lerp(result2, noteMap1B.xyz, shapeMask);
                
                // Join both sides together
                float3 result3 = lerp(result, result2, sideMask);

                // Apply Chains
                result3 = _IsChain * (chainMask * result3) * -0.5 + result3;

                // Get Bonus Triangles
                float triangleMask = _IsBonus ? (i.uv1.y > 0.999 ? noteMap2C.z : noteMap2C.w) : 0;
                float3 result4 = lerp(result3, result3 * (1 - color) - (1 - color) + 1, triangleMask);
                
                return float4(result4, 1);
            }
            ENDCG
        }
    }
}