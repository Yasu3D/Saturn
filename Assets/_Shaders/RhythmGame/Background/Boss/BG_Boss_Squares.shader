Shader "SaturnGame/RhythmGame/Backgrounds/Boss Squares  "
{
    Properties
    {
        [NoScaleOffset] _SquareMap ("Square Map", 2D) = "white" {}
        [NoScaleOffset] _GradientMap ("Gradient Map", 2D) = "white" {}
        
        _BPM ("BPM", Float) = 120
        _VisualTime ("Visual Time", Float) = 0
        _Color ("Color", Color) = (0.698, 0, 0.107, 1)
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

            sampler2D _SquareMap;
            sampler2D _GradientMap;
            
            float4 _SquareMap_ST;
            float4 _GradientMap_ST;

            float _BPM;
            float _VisualTime;

            float4 _Color;
            
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

            float GetStripes(float2 uv)
            {
                float2 scaled = uv * 8 - 0.5;
                float2 rotated = float2(
                    dot(scaled, float2(0.588, -0.809)),
                    dot(scaled, float2(0.809, 0.588)));
                
                rotated += 0.5;

                return tex2D(_GradientMap, rotated).x;
            }

            float GetSquareOuter(float2 uv, float time)
            {
                if (time < 0.5) return 0;
                float2 scaled = (uv - 0.5) / time + 0.5;
                
                return tex2D(_SquareMap, scaled);
            }

            float GetSquareInner(float2 uv, float time)
            {
                if (time < 1) return 0;
                float2 scaled = (uv - 0.5) / (time - 1) * 0.5 + 0.5;
                
                return tex2D(_SquareMap, scaled);
            }
            
            float4 frag(v2f i) : SV_Target
            {
                const float time = _VisualTime * 0.001 * (_BPM / 60) % 4;

                float stripes = GetStripes(i.uv);
                float squareOuter = GetSquareOuter(i.uv, time);
                float squareInner = GetSquareInner(i.uv, time);
                
                clip(squareOuter - squareInner - stripes - 0.5);
                
                return _Color;
            }
            ENDCG
        }
    }
}