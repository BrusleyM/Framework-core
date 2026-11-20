Shader "Framework/Samples/Iridescent"
{
    Properties
    {
        [HDR] _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 8)) = 1.6
        _Scale ("Colour scale", Range(0, 6)) = 2.2
        _Speed ("Colour speed", Range(-2, 2)) = 0.25
        _Fresnel ("Fresnel power", Range(0.5, 6)) = 2
        _Brightness ("Base brightness", Range(0, 1)) = 0.35
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
                half4 _Tint;
                half _Intensity;
                half _Scale;
                half _Speed;
                half _Fresnel;
                half _Brightness;
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

            float3 Palette(float t)
            {
                return 0.5 + 0.5 * cos(6.28318 * (t + float3(0.0, 0.33, 0.67)));
            }

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
                float facing = saturate(dot(normal, viewDirection));
                float fresnel = pow(1.0 - facing, _Fresnel);
                float hue = fresnel * _Scale + input.positionWS.y * 0.12 + _Time.y * _Speed;
                float3 rainbow = Palette(hue);
                float3 color = rainbow * lerp(_Brightness, 1.0, fresnel) * _Intensity * _Boost * _Tint.rgb;
                color += rainbow * _Flash;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
