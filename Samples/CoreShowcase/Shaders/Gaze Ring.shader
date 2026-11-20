Shader "Framework/Samples/Gaze Ring"
{
    Properties
    {
        [HDR] _Color ("Progress colour", Color) = (0.0, 2.5, 3.0, 1.0)
        [HDR] _DoneColor ("Done colour", Color) = (0.4, 3.0, 0.8, 1.0)
        _Fill ("Fill", Range(0, 1)) = 0
        _Complete ("Complete", Range(0, 1)) = 0
        _InnerRadius ("Inner radius", Range(0.1, 0.49)) = 0.38
        _OuterRadius ("Outer radius", Range(0.1, 0.5)) = 0.46
        _TrackStrength ("Track strength", Range(0, 1)) = 0.18
        _DotRadius ("Centre dot radius", Range(0, 0.1)) = 0.035
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Overlay" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _DoneColor;
                half _Fill;
                half _Complete;
                half _InnerRadius;
                half _OuterRadius;
                half _TrackStrength;
                half _DotRadius;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centred = input.uv - 0.5;
                float radius = length(centred);
                float edge = fwidth(radius) * 1.5;
                float ring = smoothstep(_InnerRadius - edge, _InnerRadius + edge, radius)
                    * (1.0 - smoothstep(_OuterRadius - edge, _OuterRadius + edge, radius));
                float turns = frac(atan2(centred.x, centred.y) / 6.2831853 + 1.0);
                float filled = step(turns, _Fill);
                float dot = 1.0 - smoothstep(_DotRadius - edge, _DotRadius + edge, radius);
                half3 colour = lerp(_Color.rgb, _DoneColor.rgb, _Complete);
                float strength = ring * lerp(_TrackStrength, 1.0, filled) + dot;
                return half4(colour, saturate(strength));
            }
            ENDHLSL
        }
    }
}
