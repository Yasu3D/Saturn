Shader "SaturnGame/UI/BgmProgressBar"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _OuterRadius ("Outer Radius", Range (0, 1)) = 0.976
        _InnerRadius ("Inner Radius", Range (0, 1)) = 0.955
        _Progress ("Progress", Range (0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Unused, just here to make the unity compiler shut up.
            Texture2D _MainTex;

            float _OuterRadius;
            float _InnerRadius;
            float _Progress;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float pi = 3.14159265359;

                // Scale UVs
                float2 uv = i.uv * 2 - 1;

                // Convert UVs from Cartesian to Polar
                float distance = length(uv);
                float angle = atan2(uv.y, uv.x);
                float2 polarCoord =  float2(angle / pi, distance);

                // Remap polar x coordinate from [-1 <> 1] to [0 <> 1]
                polarCoord.x = (polarCoord.x + 1) * 0.5;

                // Get radial progress
                float progressMask = step(_Progress, polarCoord.x);

                // Get inner and outer ring circles
                float outside = step(polarCoord.y, _OuterRadius);
                float inside = step(_InnerRadius, polarCoord.y);
                float ring = min(inside, outside);

                float alpha = ring * progressMask * i.color.w;

                return float4(i.color.xyz, alpha);

            }

            ENDCG
        }
    }
}