Shader "Framework/Samples/Neon Glow"
{
    Properties
    {
        [HDR] _GlowColor ("Glow colour", Color) = (0.0, 1.0, 1.0, 1.0)
        [HDR] _CoreColor ("Core colour", Color) = (0.02, 0.04, 0.07, 1.0)
        _Intensity ("Intensity", Range(0, 12)) = 3
        _RimPower ("Rim power", Range(0.5, 8)) = 2.5
        _Fill ("Base fill", Range(0, 1)) = 0.15
        _PulseSpeed ("Pulse speed", Range(0, 10)) = 2
        _PulseAmount ("Pulse amount", Range(0, 1)) = 0.25
        _Boost ("Boost", Range(0, 4)) = 1
        _Flash ("Flash", Range(0, 6)) = 0
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
                half4 _GlowColor;
                half4 _CoreColor;
                half _Intensity;
                half _RimPower;
                half _Fill;
                half _PulseSpeed;
                half _PulseAmount;
                half _Boost;
                half _Flash;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float rim = pow(1.0 - saturate(dot(normal, viewDirection)), _RimPower);
                float pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed + input.positionWS.y * 2.0);
                float glow = saturate(rim + _Fill);
                float3 color = lerp(_CoreColor.rgb, _GlowColor.rgb * _Intensity * _Boost, glow) * pulse;
                color += _GlowColor.rgb * _Flash;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
