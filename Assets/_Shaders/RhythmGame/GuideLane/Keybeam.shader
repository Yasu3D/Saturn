Shader "SaturnGame/RhythmGame/Keybeam"
{
    Properties
    {
        [IntRange] _NoteWidth ("Note Width", Range(1, 5)) = 3
        [Toggle] _ClipJudgementLine ("Clip Judgement Line", Integer) = 1
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

            int _NoteWidth;
            bool _ClipJudgementLine;
            
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
                const float outerEdge[] = {0.2019, 0.201, 0.1985, 0.1961, 0.1936};
                const float innerEdge[] = {0.2083, 0.2097, 0.212, 0.2146, 0.2169};
                
                const bool mask = !(i.uv.y > outerEdge[_NoteWidth - 1] && i.uv.y < innerEdge[_NoteWidth - 1] && _ClipJudgementLine);
                clip(mask - 0.5);

                const float gradientStart = 0.33;
                const float gradientEnd = 0.185;
                float gradient = clamp((i.uv.y - gradientStart) / (gradientEnd - gradientStart), 0, 1);
                gradient = pow(gradient, 4);
                
                return float4(1, 1, 1, gradient);
            }
            ENDCG
        }
    }
}