Shader "SaturnGame/RhythmGame/Backgrounds/Standard_Particle_B"
{
    Properties
    {
        _DotMap ("Dot Map", 2D) = "white" {}
        _BPM ("BPM", Float) = 120
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

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _DotMap;
            float4 _DotMap_ST;

            float _BPM;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                const float time = _Time.y * (_BPM / 120) % 0.5;
                const float2 scaledUV = (i.uv - 0.5) * (-time - 1) + 0.5;

                const float dot = tex2D(_DotMap, i.uv).x;
                const float ring = tex2D(_DotMap, scaledUV).y;

                clip(dot + ring - 0.5);
                return float4(0.4392, 0.6431, 1, 1);
            }
            ENDCG
        }
    }
}
