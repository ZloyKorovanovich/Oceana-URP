/*
Copyright 2024 Mitrofan Juryev

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

Shader "Oceana/OceanUnderwater"
{
    Properties
    {
        [Header(Maps)]
        [HideInInspector]_Scroll_0 ("Scroll 0", 2D) = "blue" {}
        [HideInInspector]_Scroll_1 ("Scroll 1", 2D) = "blue" {}
        [HideInInspector]_Scroll_2 ("Scroll 2", 2D) = "blue" {}

        [Header(Cascades)]
        [HideInInspector]_Height_0 ("Height 0", float) = 1
        [HideInInspector]_Height_1 ("Height 1", float) = 1
        [HideInInspector]_Height_2 ("Height 2", float) = 1

        [Header(Fog)]
        _Color ("Color", color) = (1, 1, 1, 1)
        _FogDistance ("Distance", float) = 0.2
        _FogPower ("Power", range(0, 1)) = 0.5

        [Header(Waterline)]
        _LineOffset ("Offset", float) = 0.001
        _LineScale ("Scale", float) = 0.01
        _LineColor ("Color", color) = (0.5, 0.5, 0.8, 1)

        [HideInInspector] _BlitTexture ("Color Texture", 2D) = "white" {}
    }
    SubShader
    {

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScaling.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "include/FullScreenTransforms.hlsl"
            #include "include/OceanScrolls.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position_cs : SV_POSITION;
                float2 uv_ss       : TEXCOORD0;
                float3 viewDir_ws      : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 position_cs = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv_ss  = GetFullScreenTriangleTexCoord(input.vertexID);

                float3 viewDir_ws = GetViewDir_WS(position_cs);

                output.position_cs = position_cs;
                output.uv_ss = uv_ss;
                output.viewDir_ws = viewDir_ws;

                return output;
            }


            uniform sampler2D _BlitTexture;
            half4 _Color, _LineColor;
            half _FogDistance, _FogPower;
            half _LineOffset, _LineScale;

            half SphereMask(float3 coords, float3 center, float radius, half hardness)
            {
                return 1 - saturate((distance(coords, center) - radius) / (1 - hardness));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 sourceColor = tex2Dlod(_BlitTexture, float4(input.uv_ss, 0, 0)).rgb;
                half sourceDepth = SampleSceneDepth(input.uv_ss);

                float3 viewDir_ws = normalize(input.viewDir_ws);

                half depth_linearEye = LinearEyeDepth(sourceDepth, _ZBufferParams);

                float3 position_ws = GetPosition_WS(depth_linearEye, viewDir_ws);
                float3 near_ws = GetPosition_WS(LinearEyeDepth(1 / _ZBufferParams.y, _ZBufferParams), viewDir_ws);
                float3 viewVector_ws = _WorldSpaceCameraPos - position_ws;

                half distanceSqr = dot(viewVector_ws, viewVector_ws);
                half fogMask = pow(saturate(distanceSqr / (_FogDistance * _FogDistance)), _FogPower);

                float3 cameraSphere = normalize(-viewVector_ws);
                float sphereMask = SphereMask(cameraSphere, _MainLightPosition.xyz, 0.5, -0.5) * (0.5f + cameraSphere.y * 0.5f);
                float cameraMask = 1 - pow(saturate(-_WorldSpaceCameraPos.y / _FogDistance), 1 - _FogPower);

                half3 fogColor = lerp(_Color.rgb, sqrt(_Color.rgb * _MainLightColor.rgb), sphereMask * cameraMask);


                half heightGradient = (SampleScrollsHeight_WS(near_ws.xz) + _LineOffset) - near_ws.y;

                half lineMask = 1 - saturate(heightGradient / _LineScale);
                half3 underwaterColor = lerp(fogColor, _LineColor.rgb, lineMask);

                half heightMask = sign(saturate(heightGradient));
                half3 color = lerp(sourceColor, underwaterColor, heightMask * saturate(fogMask + lineMask));

                return half4(color, 1);
            }

            ENDHLSL
        }
    }
}
