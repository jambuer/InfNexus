// Kopyalanacak Shader Kodu (UI_AdvancedEffects.shader)
Shader "UI/AdvancedEffects"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Yeni Efekt Ayarlarımız
        [Header(Effects)]
        _Saturation ("Saturation (Solgunluk)", Range(0, 2)) = 1
        _Brightness ("Brightness (Parlaklık)", Range(0, 3)) = 1
        _Contrast ("Contrast (Karşıtlık)", Range(1, 3)) = 1
        _BlurAmount ("Blur Amount (Bulanıklık)", Range(0, 5)) = 0

        // UI Maskeleme için gerekli
        [Header(Advanced UI)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0
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

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _MainTex_TexelSize;
            float _Saturation;
            float _Brightness;
            float _Contrast;
            float _BlurAmount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;
                
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Doygunluk (Saturation) hesaplaması
            fixed4 ApplySaturation(fixed4 color, float saturation)
            {
                // Luminance (parlaklık) katsayıları
                float lum = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
                float3 grayscale = float3(lum, lum, lum);
                // Doygunluğu ayarla (0 = siyah-beyaz, 1 = normal)
                color.rgb = lerp(grayscale, color.rgb, saturation);
                return color;
            }

            // Parlaklık (Brightness)
            fixed4 ApplyBrightness(fixed4 color, float brightness)
            {
                color.rgb *= brightness;
                return color;
            }

            // Kontrast (Contrast)
            fixed4 ApplyContrast(fixed4 color, float contrast)
            {
                // Orta griye göre kontrastı ayarla
                color.rgb = (color.rgb - 0.5) * contrast + 0.5;
                return color;
            }
            
            // Basit Box Blur (Bulanıklık)
            fixed4 ApplyBlur(sampler2D tex, float2 uv, float blurAmount)
            {
                if (blurAmount <= 0)
                {
                    return tex2D(tex, uv); // Blur yoksa orijinal pikseli döndür
                }

                float4 col = float4(0,0,0,0);
                float2 pixelSize = _MainTex_TexelSize.xy * blurAmount;

                // 9-nokta (3x3) örnekleme
                col += tex2D(tex, uv + float2(-pixelSize.x, -pixelSize.y));
                col += tex2D(tex, uv + float2(0, -pixelSize.y));
                col += tex2D(tex, uv + float2(pixelSize.x, -pixelSize.y));

                col += tex2D(tex, uv + float2(-pixelSize.x, 0));
                col += tex2D(tex, uv + float2(0, 0));
                col += tex2D(tex, uv + float2(pixelSize.x, 0));

                col += tex2D(tex, uv + float2(-pixelSize.x, pixelSize.y));
                col += tex2D(tex, uv + float2(0, pixelSize.y));
                col += tex2D(tex, uv + float2(pixelSize.x, pixelSize.y));

                return col / 9.0;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Blur'u uygula
                fixed4 color = ApplyBlur(_MainTex, IN.texcoord, _BlurAmount);
                
                // 2. Renk efektlerini uygula
                color = ApplySaturation(color, _Saturation);
                color = ApplyBrightness(color, _Brightness);
                color = ApplyContrast(color, _Contrast);

                // 3. UI Tint ve Alfa ile birleştir
                color *= IN.color;

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}