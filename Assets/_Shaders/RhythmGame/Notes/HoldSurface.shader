Shader "SaturnGame/RhythmGame/NoteRendering/HoldSurface"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Gradient Map", 2D) = "white" {}
        _ColorIndex ("Color ID", Integer) = 0
        _ConeBounds ("Cone Bounds", Vector) = (0, -6, 0, 0)
        _State ("State", Integer) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
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
                float4 screen : TEXCOORD1;
                float3 world : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ConeBounds;
            int _ColorIndex;
            int _State;
            
            v2f vert (appdata v)
            {
                v2f output;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                if (worldPos.z < _ConeBounds.y) worldPos = float3(0, 0, _ConeBounds.y);
                else worldPos = float3(worldPos.xy * ((worldPos.z - _ConeBounds.y) / (_ConeBounds.x - _ConeBounds.y)), worldPos.z);
                
                output.uv = v.uv * float2(0, 1) + float2(_ColorIndex / 13.0 + 0.05, 0);
                output.clip = UnityWorldToClipPos(worldPos);
                output.screen = ComputeScreenPos(output.clip);
                output.world = worldPos;
                
                return output;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 gradient = tex2D(_MainTex, i.uv).xyz;
                float4 result;

                switch (_State)
                {
                default:
                    {
                        result = float4(gradient, 0.6);
                        break;
                    }
                case 1:
                    {
                        float3 activeGradient = float3(1, 1, 1) - gradient;
                        activeGradient = -activeGradient * activeGradient + float3(1, 1, 1);
                        
                        result = float4(activeGradient, 0.6 * (i.world.z < _ConeBounds.x));
                        break;
                    }
                case 2:
                    {
                        float luminance = dot(gradient.xyz, float3(0.3, 0.59, 0.11));
                        float3 transformedColor = (luminance - gradient.xyz) * 0.4 + gradient.xyz;
                        result = float4(transformedColor * 0.5, 0.75);
                        break;
                    }
                }

                return result;
            }
            ENDCG
        }
    }
}
