Shader "SaturnGame/RhythmGame/Backgrounds/Standard"
{
    Properties
    {
        [NoScaleOffset] _TileMap ("Tile Map", 2D) = "white" {}
        [NoScaleOffset] _TriangleMap ("Triangle Map", 2D) = "white" {}
        [NoScaleOffset] _GlowMap ("Glow Map", 2D) = "white" {}
        
        _Scroll_A ("Triangle Scroll", Float) = 0.15
        _Scroll_B ("Stripe Scroll", Float) = 0.3
        
        _Color_Triangle ("Triangle Color", Color) = (0, 0, 0, 1)
        _Color_Stripes ("Stripe Color", Color) = (0, 0, 0, 1)
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

            sampler2D _TileMap;
            sampler2D _TriangleMap;
            sampler2D _GlowMap;
            
            float4 _TriangleMap_ST;
            float4 _TileMap_ST;
            float4 _GlowMap_ST;

            float _Scroll_A;
            float _Scroll_B;
            
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

            float flashTriangles(float4 triangleMap)
            {
                float t = saturate(sin(UNITY_PI * _Time.y) + 0.5);
                float mask = triangleMap.y - triangleMap.x;
                mask = t * mask + triangleMap.x;
                mask = saturate(mask + 0.35);
                return mask * triangleMap.z;
            }
            
            float2 rotateUV(float2 uv, float degrees)
            {
                const float Deg2Rad = UNITY_PI / 180.0;

                float radians = degrees * Deg2Rad;
                float sine = sin(radians);
                float cosine = cos(radians);
                
                uv -= 0.5;
                uv = mul(float2x2( cosine, -sine, sine, cosine), uv);
                uv += 0.5;
 
                return uv;
            }

            float2 spinPulseUV(float2 uv)
            {
                float t = -4 * _Time.y;
                float sine = sin(t);
                float cosine = cos(t);
                
                float2 spin = float2(
                    dot(float2(cosine, -sine), uv - 0.5),
                    dot(float2(sine, cosine), uv - 0.5)
                    );

                return float2(0.5, 0.5) + uv + spin;
            }

            float3 getFancyPurpleGradient(float distanceToCenter)
            {
                const float3 blue = float3(0.25146, 0.21936, 2.43164);
                const float3 purple = float3(0.03889, 0.00198, 0.06671);
                
                float mask = 1 - 1 / exp2(pow(1 - distanceToCenter * 2, 2) * 1.443);
                mask *= step(distanceToCenter * 2, 1);
                
                return blue * mask + purple;
            }
            
            float4 getTriangleMap(float2 uv0, float2 uv1, float distanceToCenter)
            {
                float2 uv = uv1 * (distanceToCenter * -3.5 + 1) + uv0 - 2;
                return tex2D(_TriangleMap, uv);
            }
            
            float4 mixGlowWithTriangles(float2 uv, float4 triangleMap, float distanceToCenter)
            {
                float3 glowMap = tex2D(_GlowMap, uv);
                float mask = glowMap.x + glowMap.y;
                float centerGlow = saturate((-distanceToCenter / 0.3 + 1) / 0.9) * 1.76;
                
                glowMap = glowMap.y * float3(-1.5, 1.77344, 20) + float3(1.595, 0.592, -0.477);
                glowMap = mask * glowMap + float3(0.10481, 0.01568, 0.67708);

                float3 backgroundGradient = getFancyPurpleGradient(distanceToCenter) * flashTriangles(triangleMap);

                float3 spinningLights = tex2D(_GlowMap, spinPulseUV(uv)).z * (-backgroundGradient + glowMap) + backgroundGradient;
                float3 combinedLights = spinningLights * float3(0.73345, 0.0897, 0.89063);

                return float4(centerGlow * (spinningLights - combinedLights) + combinedLights, 1);
            }
            
            float4 addBlueGrid(float2 uv, float4 input)
            {
                const float3 blue = float3(0, 0.21708, 1);
                const float3 purple = float3(0.06692, 0, 0.8);
                
                float4 tileMap = tex2D(_TileMap, uv);
                float3 blueGrid = tileMap.z * blue;

                float4 triangleMap = tex2D(_TriangleMap, uv);
                float3 purpleTiles = triangleMap.w * purple * tileMap.w;
                
                return float4(input.xyz + (blueGrid * 0.35 + purpleTiles), 1);
            }

            float4 addScrollingStripes(float2 uv, float4 input)
            {
                float2 uv_triangle = rotateUV(uv, 30);
                float2 uv_dashes = rotateUV(uv, 30);
                
                uv_triangle.y += _Scroll_A * _Time.y;
                uv_dashes.y += _Scroll_B * _Time.y;
                
                float stripes_triangle = tex2D(_TileMap, uv_triangle).x;
                float stripes_parallel = tex2D(_TileMap, uv_dashes).y;

                float3 result = lerp(input, float3(1, 0.03012, 0.41943), stripes_triangle);
                return float4(lerp(result, float3(0.5918, 0.41235, 1), stripes_parallel), 1);
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float2 triangleUV0 = i.uv * 4;
                float2 triangleUV1 = i.uv - 0.5;

                float distanceToCenter = sqrt(dot(triangleUV1, triangleUV1));

                float4 result = getTriangleMap(triangleUV0, triangleUV1, distanceToCenter);
                result = mixGlowWithTriangles(i.uv, result, distanceToCenter);
                result = addBlueGrid(i.uv, result);
                result = addScrollingStripes(i.uv, result);
                
                return result;
            }
            ENDCG
        }
    }
}