Shader "Framework/Samples/Hologram"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (0.0, 0.8, 1.0, 1.0)
        _Intensity ("Intensity", Range(0, 8)) = 2
        _Alpha ("Alpha", Range(0, 1)) = 0.6
        _BaseAlpha ("Base alpha", Range(0, 1)) = 0.12
        _FresnelPower ("Fresnel power", Range(0.5, 8)) = 2
        _ScanDensity ("Scanline density", Range(0, 80)) = 28
        _ScanSpeed ("Scanline speed", Range(-8, 8)) = 1.5
        _ScanStrength ("Scanline strength", Range(0, 1)) = 0.45
        _FlickerSpeed ("Flicker speed", Range(0, 30)) = 12
        _FlickerAmount ("Flicker amount", Range(0, 1)) = 0.12
        _BorderWidth ("Border width (uv)", Range(0, 0.3)) = 0
        _BorderStrength ("Border strength", Range(0, 4)) = 1.5
        _Boost ("Boost", Range(0, 4)) = 1
        _Flash ("Flash", Range(0, 6)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _Alpha;
                half _BaseAlpha;
                half _FresnelPower;
                half _ScanDensity;
                half _ScanSpeed;
                half _ScanStrength;
                half _FlickerSpeed;
                half _FlickerAmount;
                half _BorderWidth;
                half _BorderStrength;
                half _Boost;
                half _Flash;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float Hash(float value)
            {
                return frac(sin(value * 127.1) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float3 normal = normalize(input.normalWS) * (isFrontFace ? 1.0 : -1.0);
                float3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float fresnel = pow(1.0 - saturate(dot(normal, viewDirection)), _FresnelPower);
                float scan = 0.5 + 0.5 * sin(input.positionWS.y * _ScanDensity - _Time.y * _ScanSpeed);
                scan = lerp(1.0, scan, _ScanStrength);
                float flicker = 1.0 - _FlickerAmount * Hash(floor(_Time.y * _FlickerSpeed));
                float edge = min(min(input.uv.x, 1.0 - input.uv.x), min(input.uv.y, 1.0 - input.uv.y));
                float border = _BorderWidth > 0.0001 ? (1.0 - smoothstep(0.0, _BorderWidth, edge)) : 0.0;
                float alpha = saturate((_BaseAlpha + fresnel) * _Alpha * scan * flicker + border * _BorderStrength * 0.5 + _Flash * 0.1);
                float3 color = _Color.rgb * (_Intensity * _Boost + _Flash);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
