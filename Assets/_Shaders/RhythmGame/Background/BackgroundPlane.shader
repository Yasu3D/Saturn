Shader "SaturnGame/RhythmGame/Background Plane"
{
    Properties
    {
        [NoScaleOffset] _RenderMap ("Render Map", 2D) = "white" {}
        [IntRange] _MaskDensity ("Mask Density", Range(0, 5)) = 0
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

            sampler2D _RenderMap;
            float4 _RenderMap_ST;

            int _MaskDensity;
            
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

            v2f vert (appdata v)
            {
                v2f output;
                
                output.uv = v.uv;
                output.clip = UnityObjectToClipPos(v.vertex);
                output.color = v.color;
                
                return output;
            }

            float4 frag (v2f i) : SV_Target
            {
                const float brightness[] = {1, 0.75, 0.5, 0.25, 0.02};
                return float4(tex2D(_RenderMap, i.uv).xyz * brightness[_MaskDensity], 1);
            }
            ENDCG
        }
    }
}
