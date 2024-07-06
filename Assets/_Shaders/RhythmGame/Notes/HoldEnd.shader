Shader "SaturnGame/RhythmGame/Notes/HoldEnd"
{
    Properties
    {
        [NoScaleOffset] _GradientMap ("Gradient Map", 2D) = "white" {}
        [NoScaleOffset] _HoldEndMap ("Hold End Map", 2D) = "white" {}
        _ColorIndex ("Color ID", Integer) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _GradientMap;
            sampler2D _HoldEndMap;
            
            float4 _GradientMap_ST;
            float4 _HoldEndMap_ST;
            
            int _ColorIndex;
            
            v2f vert (appdata v)
            {
                v2f output;
                
                output.uv = v.uv;
                output.clip = UnityObjectToClipPos(v.vertex);
                
                return output;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 colorUV = float2(_ColorIndex / 13.0 + 0.05, 0.7);
                float3 color = tex2D(_GradientMap, colorUV).xyz;

                float mask = tex2D(_HoldEndMap, i.uv).y;
                color = lerp(color * 0.5, color, mask);
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
