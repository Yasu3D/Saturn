Shader "SaturnGame/RhythmGame/Notes/SnapArrow"
{
    Properties
    {
        [NoScaleOffset] _SnapMap ("Snap Map", 2D) = "white" {}
        [Toggle] _FlipArrow ("Flip Arrow", Integer) = 0
        _NoteColor ("Note Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _SnapMap;
            float4 _SnapMap_ST;
            float4 _NoteColor;

            int _FlipArrow;
            
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
                float2 uv = float2(_FlipArrow ? 1 - i.uv.x : i.uv.x, i.uv.y);
                float4 snapArrow = tex2D(_SnapMap, uv);
                clip(snapArrow.z - 0.3);
                
                const float uvGradient = (1 - uv.x) * (1 - uv.x);
                const float invertedUvGradient = 1 - uvGradient;
                
                const float3 color = pow(_NoteColor.xyz, 1 / 2.2) * 0.5;
                const float3 colorSquared = color * color;
                const float3 colorInverted2 = 1 - color * 2;
                const float3 colorInverted3 = 1 - color * 3;
                
                float3 colorGradient = lerp(color, colorSquared, uvGradient);
                colorGradient *= snapArrow.y;
                
                float3 arrowBody = -invertedUvGradient * colorInverted3 + 1 + colorGradient * colorGradient;
                arrowBody *= snapArrow.x;
                
                float3 result = 1 - arrowBody;
                result = -result * colorInverted2 + 1;
                result = (0.2 + arrowBody) * result * 2;

                return float4(result, 1);
            }
            ENDCG
        }
    }
}