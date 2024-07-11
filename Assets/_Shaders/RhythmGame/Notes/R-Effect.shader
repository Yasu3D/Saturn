Shader "SaturnGame/RhythmGame/Notes/R-Effect"
{
    Properties
    {
        [NoScaleOffset] _EffectMap ("Effect Map", 2D) = "white" {}
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

            sampler2D _EffectMap;
            float4 _EffectMap_ST;
            
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
                const float mask = tex2D(_EffectMap, i.uv);
                float3 color = mask * float3(2.5, 2.3, 1.04);
                
                return float4(color, mask);
            }
            ENDCG
        }
    }
}