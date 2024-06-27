Shader "SaturnGame/RhythmGame/Backgrounds/Standard_Particle_A"
{
    Properties
    {
        _DotMap ("Dot Map", 2D) = "white" {}
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
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            sampler2D _DotMap;
            float4 _DotMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = v.uv0;
                o.uv1 = v.uv1;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // vertex color:
                // x = general visibility multiplier
                // w = age
                
                const float2 uvScale = float2(1, 23.5);
                const float4 color = float4(0.4392, 0.6431, 1, 1);

                float dotMap = tex2D(_DotMap, i.uv0 * uvScale).x;

                float size = 1 - abs(sin((i.uv1.y + i.color.w) * 3));
                float alpha = step(dotMap, size * i.color.x);

                clip(alpha - 0.5);
                return color;
            }
            ENDCG
        }
    }
}
