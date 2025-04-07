Shader "Oceana/WaterUnderwater" {

    Properties{
        _WaterLineOffset ("Water Line Offset", float) = 0
        _ColorDepth ("Depth Color", color) = (0.00, 0.001, 0.01, 1.0)
        _FogColor ("Fog Color", color) = (0.6, 0.1, 0.3, 1.0)
        _FogDensity ("Fog Density", range(0, 1)) = 0.5
        _LuminanceEdgeHigh ("Luminance High Edge", range(0, 1)) = 0.3
        _LuminanceEdgeLow ("Luminance Low Edge", range(0, 1)) = 0.4
        _LightFalloff ("Light Falloff", range(0, 1)) = 0.2

        _GodraysSampleDistance ("Godrays Sample Distance", float) = 2
    }

    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline"}

        HLSLINCLUDE
        cbuffer UnityPerMaterial {
            uniform float _WaterLineOffset;

            uniform float4 _FogColor;
            uniform float4 _ColorDepth;
            uniform float _FogDensity;

            uniform float _LuminanceEdgeHigh;
            uniform float _LuminanceEdgeLow;
            uniform float _LightFalloff;

            uniform float _GodraysSampleDistance;
        }

        cbuffer FromDynamic {
            uniform float4 _ScrollMap_ST;
            uniform float _SeaLevel;
            uniform float _DisplaceHeight;

            uniform float4x4 unity_MatrixInvV;
            uniform float4x4 unity_MatrixInvP;
            uniform float4x4 unity_MatrixInvVP;
            
            uniform float4x4 unity_MatrixV;
            uniform float4 _WorldSpaceCameraPos;

            uniform float4 _ProjectionParams;
            uniform float4 _ScreenParams;
            uniform float4 _ZBufferParams;

            uniform float4 _MainLightPosition;
            uniform float4 _MainLightColor;

            uniform SamplerState _pointClampSampler;
            uniform SamplerState _bilinearRepeatSampler;
        }

        tbuffer MapBuffer {
            uniform Texture2D _ScrollMap;
            uniform Texture2D _SourceColor;
            uniform Texture2D _SourceDepth;
        }

        #include "include/ScreenSpaceFunctions.hlsl"
        #include "include/MapPacking.hlsl"

        ENDHLSL

        Pass {
            Cull [Off]
            ZTest [Always]
            ZWrite [Off]

            HLSLPROGRAM
            #pragma vertex VertFunc
            #pragma fragment FragFunc

            const static float QuadTable[12] = {-1.0, -1.0,
                                                -1.0,  1.0,
                                                 1.0, -1.0,
                                                 1.0, -1.0,
                                                -1.0,  1.0,
                                                 1.0,  1.0};
            
            struct Varyings {
                float4 positionSS : TEXCOORD0;
                float4 positionWS : TEXCOORD1;
                float4 positionCS : SV_POSITION;
            };

            Varyings VertFunc(uint vertexID : SV_VertexID) {
                Varyings output = (Varyings)0;
                output.positionCS = float4(QuadTable[vertexID * 2] * _ProjectionParams.x, QuadTable[(vertexID + 1) * 2] * _ProjectionParams.x, 1, 1);
                output.positionSS = TransformClipToScreen(output.positionCS, _ProjectionParams);
                float4 hpositionWS = mul(unity_MatrixInvVP, output.positionCS);
                output.positionWS = float4(hpositionWS.xyz / hpositionWS.w, 1); 
                return output;
            }

            float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth) {
                float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
                positionCS.y = -positionCS.y;
                return positionCS;
            }

            float4 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix) {
                float4 positionCS  = ComputeClipSpacePosition(positionNDC, deviceDepth);
                float4 hpositionWS = mul(invViewProjMatrix, positionCS);
                return float4(hpositionWS.xyz / hpositionWS.w, LinearEyeDepth(deviceDepth, _ZBufferParams));
            }

            float4 FragFunc(Varyings input) : SV_TARGET {
                float2 uvSS = TransformScreenToUV(input.positionSS);

                float3 sceneColor = _SourceColor.SampleLevel(_pointClampSampler, uvSS, 0).rgb;
                float sceneDepth = _SourceDepth.SampleLevel(_pointClampSampler, uvSS, 0).r;

                float4 positionWS = ComputeWorldSpacePosition(input.positionSS.xy * _ScreenParams.zw, sceneDepth, unity_MatrixInvVP);
                float3 viewDir = normalize(input.positionWS.xyz - _WorldSpaceCameraPos.xyz);

                float heightSample = _ScrollMap.Sample(_bilinearRepeatSampler, input.positionWS.xz * _ScrollMap_ST.xy + _ScrollMap_ST.zw).a;
                float height = _SeaLevel + heightSample * _DisplaceHeight;

                float lightAbsorption = saturate(1 / pow(2.71828, pow(max(-_WorldSpaceCameraPos.y, 1), max(_LightFalloff, 0.01)) * max(_FogDensity, 0.001)));
                float luminanceMask = smoothstep(_LuminanceEdgeLow, _LuminanceEdgeHigh, saturate(saturate((viewDir.y + 1) * 0.5))) * saturate(_MainLightPosition.y);
                float3 fogColor = lerp(_ColorDepth.rgb, _FogColor.rgb, lightAbsorption);
                fogColor = lerp(fogColor, sqrt(_FogColor.rgb * _MainLightColor.rgb), luminanceMask * lightAbsorption);

                float fogVisibility = 1 / pow(2.71828, positionWS.w * max(_FogDensity, 0.001));
                float3 underwaterColor = lerp(fogColor, sceneColor, fogVisibility);

                return float4(lerp(underwaterColor, sceneColor, input.positionWS.y > height) , 1);
            }
            ENDHLSL
        }
    }
}