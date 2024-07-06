Shader "SaturnGame/RhythmGame/Notes/Barline"
{
    Properties { }
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
                float4 clip : SV_POSITION;
                float4 color : COLOR;
                float3 world : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.clip = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float GetAlpha(float2 uv, float3 world)
            {
                float alpha = UNITY_TWO_PI * uv.x;
                alpha = cos(alpha);
                alpha = 0.2 - alpha;
                float depth = max(0, -world.z / 4 + 0.12);
                depth = depth * depth * depth;
                depth = min(1, depth);
                float mixed = depth * (3 - alpha) + alpha;
                mixed = floor(mixed);
                mixed = mixed - 0.333333;
                mixed = mixed > 0;
                
                return mixed;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 color = float4(0.46606, 0.46802, 0.5, 1);
                clip(GetAlpha(i.uv, i.world) - 0.5);
                return color;
            }
            ENDCG
        }
    }
}
