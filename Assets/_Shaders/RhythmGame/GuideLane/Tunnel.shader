Shader "SaturnGame/RhythmGame/Tunnel"
{
    Properties
    {
        [NoScaleOffset] _JudgeLineMap ("Judge Line Map", 2D) = "white" {}
        [NoScaleOffset] _ColorMap ("Color Map", 2D) = "white" {}
        [NoScaleOffset] _GradientMap ("Gradient Map", 2D) = "white" {}
        
        [IntRange] _NoteWidth ("Note Width", Range(1, 5)) = 3
        [IntRange] _TunnelOpacity ("Tunnel Opacity", Range(0, 5)) = 5
        [IntRange] _LaneType ("Lane Type", Range(0, 7)) = 5
        [Toggle] _ComboShine ("Combo Shine", Integer) = 0
    }
    SubShader
    {
        Tags
        { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _JudgeLineMap;
            sampler2D _ColorMap;
            sampler2D _GradientMap;
            
            float4 _JudgeLineMap_ST;
            float4 _ColorMap_ST;
            float4 _GradientMap_ST;

            int _NoteWidth;
            int _LaneType;
            int _ComboShine;
            int _TunnelOpacity;
            
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

            float GetTunnelStripes(float2 uv)
            {
                float2 scaledUV = uv * float2(1, 10) + float2(0, _Time.y * 6);
                float stripes = tex2D(_JudgeLineMap, scaledUV).z;
                float depth = exp2(9 * log2(1 - uv.y));
                return clamp(stripes * depth, 0, 0.003);
            }

            float4 GetJudgelineColor(float4 clip)
            {
                float2 screenUV = clip.xy / float2(_ScreenParams.x, _ScreenParams.y);
                return tex2D(_ColorMap, screenUV);
            }

            float3 BlendStripes(float2 uv, float stripes)
            {
                const float3 backgroundColor = float3(0.0122, 0.0133, 0.03125);
                return backgroundColor - (GetTunnelStripes(uv) + clamp(stripes, 0, 0.0015));
            }

            float3 AddGuideLines(float3 input, float lineMask)
            {
                const float3 lineColor = float3(0.04443, 0.0476, 0.09375);
                return lerp(input, lineColor, lineMask);
            }

            float GetLineMask(float2 uv)
            {
                const float offset[]   = {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5};
                const float interval[] = {0, 1,   2,   3,   4,   5,  10,  15};
                float lineMask = saturate(1 - floor(uv.x + offset[_LaneType]) % interval[_LaneType]);
                return tex2D(_JudgeLineMap, uv).y * 5 * lineMask;
            }
            
            float3 AddComboShine(float3 input, float lineMask, float2 uv)
            {
                const float3 brightColor = float3(0.0443, 0.04736, 0.09375); //9
                const float3 darkColor = float3(0.28906, 0.42871, 0.5); // 10
                
                float2 scaledUV = uv * float2(1, -8) - float2(0, _Time.y * 2);
                float gradient = tex2D(_GradientMap, scaledUV).x;

                return input + lerp(brightColor, darkColor, gradient) * _ComboShine * lineMask;
            }

            float GetJudgeLineGradient(float2 uv)
            {
                const float thickness[] = {19000, 11000, 4600, 2500, 1600};
                
                float result = uv.y * 0.5 + 0.1473;
                result = sin(result * UNITY_TWO_PI);
                result = max(0, result);
                result = log2(result);
                result *= thickness[_NoteWidth - 1];
                result = min(1, exp2(result));
                
               return result;
            }

            float GetJudgeLine(float gradient)
            {
                float mask2 = gradient * 3;
                float mask3 = -mask2 * 0.12 + 1;
                mask3 -= 0.6;
                mask3 = saturate(2.5 * mask3);

                float mask4 = mask3 * -2 + 3;
                mask3 = mask3 * mask3;
                mask3 = mask4 * mask3 + 0.2309136;
                
                return saturate(saturate(mask2 * 0.12 + floor(mask2)) * mask3);
            }
            
            float3 AddJudgeLine(float3 input, float3 judgeLineColor, float judgeLineGradient, float judgeLine)
            {
                float3 color = judgeLineColor * 1.5 + input;
                float3 result = judgeLine * color + input;
                return saturate(result);
            }

            float GetAlpha(float4 clip, float judgeLineGradient, float judgeLine)
            {
                const float opacity[] = {0, 0.7, 0.85, 0.9, 0.95, 1}; // these values are eyeballed.
                
                float alpha = clip.w / 6;
                alpha = max(0, alpha);
                alpha = log2(alpha);
                alpha *= 3;
                alpha = min(1, exp2(alpha));
                alpha = alpha * -0.3 + opacity[_TunnelOpacity];
                
                float alpha2 = frac(judgeLineGradient);
                alpha2 = frac(alpha2);
                alpha2 = max(0.5, alpha2);
                alpha2 *= UNITY_TWO_PI;
                alpha2 = cos(alpha2);
                alpha2 += 1;
                alpha2 = alpha2 * 0.5 + ceil(judgeLineGradient);

                float judgeLineGradient2 = judgeLineGradient * 3;
                judgeLineGradient2 = judgeLineGradient2 * 0.12 + floor(judgeLineGradient2);
                
                alpha2 *= judgeLineGradient2;
                
                alpha2 = saturate(alpha2 * (5 - judgeLine)) + alpha;
                
                return alpha2;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 judgeLineColor = GetJudgelineColor(i.clip);
                float guideLineMask = GetLineMask(i.uv);
                float judgeLineGradient = GetJudgeLineGradient(i.uv);
                float judgeLine = GetJudgeLine(judgeLineGradient);
                
                float3 result = BlendStripes(i.uv, judgeLineColor.w);
                result = AddGuideLines(result, guideLineMask);
                result = AddComboShine(result, guideLineMask, i.uv);
                result = AddJudgeLine(result, judgeLineColor, judgeLineGradient, judgeLine);

                float alpha = GetAlpha(i.clip, judgeLineGradient, judgeLine);
                
                //return float4(alpha, alpha, alpha, 1);
                
                return float4(result, alpha);
            }
            ENDCG
        }
    }
}