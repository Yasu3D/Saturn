// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _Spread ("Blur Spread", Range(0.1, 20)) = 0
        _GridSize ("Blur Grid Size", Integer) = 4
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        GrabPass { "_GrabTexture" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        CGINCLUDE
        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
        #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
		
        #define E 2.71828f

        sampler2D _MainTex;
        float4 _MainTex_ST;

        sampler2D _GrabTexture;
        float4 _GrabTexture_TexelSize;
            
        float4 _Color;
        float4 _TextureSampleAdd;
        float4 _ClipRect;

        float _Spread;
        int _GridSize;

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 clip   : SV_POSITION;
            float4 color    : COLOR;
            float2 texcoord  : TEXCOORD0;
            float4 world : TEXCOORD1;
            float2 screen : TEXCOORD2;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert(appdata_t v)
        {
            v2f output;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
            output.world = v.vertex;
            output.clip = UnityObjectToClipPos(v.vertex);
            output.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

            output.screen = (output.clip.xy / output.clip.w) * 0.5 + 0.5;
            output.screen.y = 1 - output.screen.y;
            
            output.color = v.color * _Color;
            return output;
        }
        ENDCG

        Pass
        {
            Name "Horizontal"
			
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            float Gaussian(int x)
            {
                float spreadSquared = _Spread * _Spread;
                return 1 / sqrt(UNITY_TWO_PI * spreadSquared) * pow(E, -(x * x) / (2 * spreadSquared));
            }
            
            float3 horizontal(v2f i)
            {
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;

                int upper = (_GridSize - 1) / 2;
                int lower = -upper;

                for (int x = lower; x <= upper; ++x)
                {
                    float gauss = Gaussian(x);
                    gridSum += gauss;
                    float2 uv = i.screen + float2(_GrabTexture_TexelSize.x * x, 0.0f);
                    col += gauss * tex2D(_GrabTexture, uv).xyz;
                }

                return col / gridSum;
            }

            float3 vertical (v2f i) : SV_Target
            {
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;

                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;

                for (int y = lower; y <= upper; ++y)
                {
                    float gauss = Gaussian(y);
                    gridSum += gauss;
                    float2 uv = i.screen + float2(0.0f, _GrabTexture_TexelSize.y * y);
                    col += gauss * tex2D(_GrabTexture, uv).xyz;
                }

                return col / gridSum;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 color = float4((horizontal(i) + vertical(i)) * 0.5, 1.0f);
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color * i.color;
            }
            ENDCG
        }
    }
}