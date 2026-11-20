Shader "Framework/Samples/Stand Glow"
{
    Properties
    {
        [HDR] _RimColor ("Rim colour", Color) = (0.0, 1.0, 1.0, 1.0)
        _BodyTop ("Body top", Color) = (0.07, 0.08, 0.12, 1)
        _BodyBottom ("Body bottom", Color) = (0.015, 0.015, 0.03, 1)
        _Intensity ("Rim intensity", Range(0, 10)) = 3.5
        _BandHeight ("Edge band height", Range(0.01, 0.4)) = 0.1
        _PulseSpeed ("Pulse speed", Range(0, 8)) = 1.6
        _StripeSpeed ("Stripe speed", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _RimColor;
                half4 _BodyTop;
                half4 _BodyBottom;
                half _Intensity;
                half _BandHeight;
                half _PulseSpeed;
                half _StripeSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float height = input.positionOS.y;
                float radius = length(input.positionOS.xz);
                float isTop = step(0.9, input.normalOS.y);
                float pulse = 0.85 + 0.15 * sin(_Time.y * _PulseSpeed);

                float topEdge = smoothstep(1.0 - _BandHeight, 1.0, height) * (1.0 - isTop);
                float bottomEdge = smoothstep(-1.0 + _BandHeight, -1.0, height) * 0.6;
                float outerRing = smoothstep(0.02, 0.0, abs(radius - 0.43)) * isTop;
                float innerRing = smoothstep(0.015, 0.0, abs(radius - 0.3)) * isTop * 0.55;
                float stripePosition = frac(height * 0.5 + 0.5 - _Time.y * _StripeSpeed);
                float stripe = smoothstep(0.06, 0.0, abs(stripePosition - 0.5)) * (1.0 - isTop) * 0.35;

                float3 body = lerp(_BodyBottom.rgb, _BodyTop.rgb, saturate(height * 0.5 + 0.5));
                float capGlow = isTop * smoothstep(0.5, 0.0, radius) * 0.1;
                float glow = (topEdge + bottomEdge + outerRing + innerRing + stripe + capGlow) * pulse;
                return half4(body + _RimColor.rgb * _Intensity * glow, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
