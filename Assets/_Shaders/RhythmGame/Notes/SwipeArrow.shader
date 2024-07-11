Shader "SaturnGame/RhythmGame/Notes/SwipeArrow"
{
    Properties
    {
        [NoScaleOffset] _ArrowMap ("Arrow Map", 2D) = "white" {}
        [NoScaleOffset] _MaskMap ("Mask Map", 2D) = "white" {}
        _NoteColor ("Note Color", Color) = (1,1,1,1)
        [Space(10)]
        [Toggle] _FlipArrow ("Flip Arrow", Integer) = 0
        [Toggle] _FlipGradient ("Flip Gradient", Integer) = 0
        [IntRange] _NoteSize ("Note Size", Range(1, 60)) = 60
    }
    SubShader
    {
        Tags
        { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ArrowMap;
            sampler2D _MaskMap;
            float4 _ArrowMap_ST;
            float4 _MaskMap_ST;
            float4 _NoteColor;

            bool _FlipArrow;
            bool _FlipGradient;
            
            int _NoteSize;
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert(appdata v)
            {
                v2f output;
                
                output.uv = v.uv;
                output.clip = UnityObjectToClipPos(v.vertex);
                
                return output;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float2 flippedUV = _FlipArrow ? 1 - i.uv : i.uv;
                float2 scrollUV = (1 - flippedUV) * float2(1, _NoteSize * 0.5) + float2(0, -0.724637695 * i.clip.w);
                
                float4 arrowMap = tex2D(_ArrowMap, scrollUV);
                float4 maskMap = tex2D(_MaskMap, flippedUV);
                
                clip(arrowMap.z * maskMap.z - 0.3);
                
                float4 color = pow(_NoteColor, 1 / 2.2) * 0.5;
                float4 colorSquared = color * color;
                float4 colorInverted2 = 1 - color * 2;
                float4 colorInverted3 = 1 - color * 3;

                float gradientUV = _FlipGradient ? (1 - flippedUV.y) * (1 - flippedUV.y) : flippedUV.y * flippedUV.y;
                float3 gradient1 = (gradientUV - 1) * colorInverted3.xyz + 1;
                float3 gradient2 = lerp(colorSquared, color, gradientUV);
                float3 result = gradient1 - gradient2;
                result *= arrowMap.x;
                result *= maskMap.x;
                result += gradient2;

                float3 intermediate = result + 0.1;
                
                result = 1 - result;
                result = -result * colorInverted2 + 1;
                result *= intermediate;
                result *= 2;
                
                return float4(result, 1);
            }
            ENDCG
        }
    }
}