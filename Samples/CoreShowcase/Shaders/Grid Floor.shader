Shader "Framework/Samples/Grid Floor"
{
    Properties
    {
        _BaseColor ("Base colour", Color) = (0.012, 0.014, 0.03, 1)
        [HDR] _LineColor ("Line colour", Color) = (0.05, 0.25, 0.7, 1)
        [HDR] _MajorColor ("Major line colour", Color) = (0.2, 0.7, 1.4, 1)
        [HDR] _RingColor ("Ring colour", Color) = (0.5, 0.2, 1.2, 1)
        _GridSize ("Grid size", Range(0.25, 4)) = 1
        _FadeDistance ("Fade distance", Range(4, 60)) = 22
        _RingSpeed ("Ring speed", Range(0, 2)) = 0.25
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
                half4 _BaseColor;
                half4 _LineColor;
                half4 _MajorColor;
                half4 _RingColor;
                half _GridSize;
                half _FadeDistance;
                half _RingSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float GridLine(float2 coordinates)
            {
                float2 derivative = fwidth(coordinates);
                float2 distanceToLine = abs(frac(coordinates - 0.5) - 0.5) / max(derivative, 0.0001);
                return 1.0 - saturate(min(distanceToLine.x, distanceToLine.y));
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 coordinates = input.positionWS.xz / _GridSize;
                float minor = GridLine(coordinates);
                float major = GridLine(coordinates / 5.0);
                float distanceFromCentre = length(input.positionWS.xz);
                float fade = saturate(1.0 - distanceFromCentre / _FadeDistance);
                fade *= fade;
                float ringPosition = frac(distanceFromCentre * 0.1 - _Time.y * _RingSpeed);
                float ring = smoothstep(0.05, 0.0, abs(ringPosition - 0.5)) * fade;
                float3 color = _BaseColor.rgb + (_LineColor.rgb * minor * 0.7 + _MajorColor.rgb * major) * fade + _RingColor.rgb * ring * 0.6;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
