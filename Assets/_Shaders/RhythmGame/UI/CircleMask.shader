Shader "SaturnGame/UI/CircleMask"
{
    Properties
    {
        _Radius ("Radius", Range (0, 1)) = 1.0
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

            float _Radius;

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

                float circle = step(_Radius, polarCoord.y);

                return float4(i.color.xyz, circle);
            }

            ENDCG
        }
    }
}