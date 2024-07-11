Shader "SaturnGame/RhythmGame/Notes/Sync"
{
    Properties
    {
        [NoScaleOffset] _SyncMap ("Sync Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _SyncMap;
            float4 _SyncMap_ST;
            
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
                float3 color = tex2D(_SyncMap, i.uv);
                color = float3(1,3,3) * color.z * color;
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
}