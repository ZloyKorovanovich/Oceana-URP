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

Shader "Oceana/OceanSurface"
{
    Properties
    {
        [Header(Maps)]
        [HideInInspector]_Scroll_0 ("Scroll 0", 2D) = "blue" {}
        [HideInInspector]_Scroll_1 ("Scroll 1", 2D) = "blue" {}
        [HideInInspector]_Scroll_2 ("Scroll 2", 2D) = "blue" {}
        [HideInInspector]_Scroll_3 ("Scroll 3", 2D) = "blue" {}

        [Header(Cascades)]
        [HideInInspector]_ScrollHeights ("Scroll Heights", vector) = (0.25, 0.25, 0.25, 0.25)

        [Header(Color)]
        _Color ("Color", color) = (0.1, 0.4, 0.6)
        _Specular ("Specular", range(0,1)) = 1
        _SpecCut("Specular Cut", range(0, 1)) = 1
        
        [Header(Environment)]
        _Fresnel ("Fresnel", float) = 10
        _Reflectivity ("Intesnity", range(0, 1)) = 1

        [Header(Scene)]
        _Refraction ("Index of Refraction", float) = 1.33
        _Depth ("Depth", float) = 5
        _DepthPower ("Depth Power", float) = 1
        _ShallowPower ("Shallow Power", float) = 1

        [Header(Foam)]
        [Toggle] _IsRednerFoam ("Render", float) = 1
        _FoamMask_0 ("Mask_0", 2D) = "black" {}
        _FoamMask_1 ("Mask_1", 2D) = "black" {}
        _FoamColor ("Color", color) = (0.8, 0.8, 0.8, 1)
        _FoamAmount ("Amount", range(0, 1)) = 0.3
        _FoamCover ("Cover", float) = 0.3
        _FoamPower ("Power", range(0.1, 2)) = 1

        [Header(Back)]
        _BackContrast ("Contrast", range(0, 1)) = 0.5
        _BackFresnel ("Fresnel", float) = 15

        [Header(SSS)]
        _SSSPower ("Power", float) = 1
        _SSSIntensity ("Intesnity", float) = 1
        _SSSNormal ("SSSNormal", range(0, 1)) = 1
        _SSSFresnel ("SSSFresnel", float) = 1

        [Header(Tessellation)]
        _TessFactor ("Factor", vector) = (15, 15, 15, 15)
        _TessDistance ("Distance", float) = 20
        _ConstRange ("Constant Height Range", float) = 10
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "Queue" = "Transparent"
        }
        
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "include/OceanScrolls.hlsl"

        ENDHLSL

        Pass
        {
            ZWrite On
            Cull [_Cull]
            AlphaToMask [_AlphaToMask]

            HLSLPROGRAM

            #pragma target 4.6

            #pragma vertex VertexFunc

            #pragma hull HS
            #pragma domain DS

            #pragma fragment FragmentFunc


            // Vertex stage
            struct Attribs
            {
                float3 position_os : POSITION;
            };

            struct VertexOut
            {
                float3 position_l : TEXCOORD0;
            };

            VertexOut VertexFunc(in Attribs input)
            {
                VertexOut o = (VertexOut)0;

                float3 position_ws = mul(UNITY_MATRIX_M, float4(input.position_os, 1)).xyz;
                o.position_l = position_ws;
                return o;
            }

            // Tessellation stages
            uniform half4 _TessFactor;
            uniform half _TessDistance;

            struct PatchTess
            {
                float edgeTess[3] : SV_TESSFACTOR;
                float insideTess : SV_INSIDETESSFACTOR;
            };

            PatchTess ConstantHS(InputPatch<VertexOut,3> patch, uint patchID : SV_PrimitiveID)
            {
                PatchTess pt = (PatchTess)0;

                float3 average0 = (patch[1].position_l + patch[2].position_l) * 0.5;
                float3 average1 = (patch[2].position_l + patch[0].position_l) * 0.5;
                float3 average2 = (patch[0].position_l + patch[1].position_l) * 0.5;

                float3 camPos = GetCameraPositionWS().xyz;

                float dist0 = distance(average0, camPos);
                float dist1 = distance(average1, camPos);
                float dist2 = distance(average2, camPos);

                float tess = sqrt(_TessDistance);

                float tess0 = 1 - saturate(sqrt(dist0) / tess);
                float tess1 = 1 - saturate(sqrt(dist1) / tess);
                float tess2 = 1 - saturate(sqrt(dist2) / tess);

                pt.edgeTess[0] = max((_TessFactor.x * tess0 * tess0), 1);
                pt.edgeTess[1] = max((_TessFactor.y * tess1 * tess1), 1);
                pt.edgeTess[2] = max((_TessFactor.z * tess2 * tess2), 1);

                pt.insideTess = (pt.edgeTess[0] + pt.edgeTess[1] + pt.edgeTess[2]) / 3;

                return pt;
            }

            struct HullOut
            {
                float3 position_l : TEXCOORD0;
            };

            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(3)]
            [patchconstantfunc("ConstantHS")]
            [maxtessfactor(64.0f)]
            HullOut HS(InputPatch<VertexOut,3> p, uint i : SV_OutputControlPointID)
            {
                HullOut o = (HullOut)0;

                o.position_l = p[i].position_l;
                return o;
            }
            
            // Domain stage
            struct DomainOut
            {
                float3 position_ws : TEXCOORD0;
                half3 viewVector_ws : TEXCOORD1;
                float4 position_ss : TEXCOORD2;

                float4 position_cs  : SV_POSITION;
            };

            half _ConstRange;

            [domain("tri")]
            DomainOut DS(PatchTess patchTess, float3  baryCoords : SV_DomainLocation, const OutputPatch<HullOut,3> triangles)
            {
                DomainOut o = (DomainOut)0;

                float3 pos_ws = triangles[0].position_l * baryCoords.x + triangles[1].position_l * baryCoords.y + triangles[2].position_l * baryCoords.z;

                //half distInfl = 1 - saturate((distance(_WorldSpaceCameraPos.xz, pos_ws.xz) - _ConstRange) / _TessDistance);
                half h = SampleScrollsHeight_WS(pos_ws.xz); // * distInfl;

                pos_ws = float3(pos_ws.x, float(h), pos_ws.z);
                float4 pos_cs = mul(UNITY_MATRIX_VP, float4(pos_ws, 1));

                o.position_ws = pos_ws;
                o.viewVector_ws = _WorldSpaceCameraPos - pos_ws;
                o.position_cs = pos_cs;
                o.position_ss = ComputeScreenPos(pos_cs);

                return o;
            }

            // Fragment stage
            half4 _Color;
            half _Specular, _SpecCut;
            half _Reflectivity, _Fresnel, _SSSFresnel;
            half _SSSPower, _SSSIntensity, _SSSNormal;
            half _Refraction, _Depth, _DepthPower, _ShallowPower;
            
            half _BackFresnel, _BackContrast;

            sampler2D _FoamMask_0, _FoamMask_1;
            float4 _FoamMask_0_ST, _FoamMask_1_ST;
            half4 _FoamColor;
            half _FoamAmount, _FoamCover, _FoamPower;
            half _IsRednerFoam;

            SamplerState bilinearRepeatSampler;

            #include "include/OceanFragmentFunctions.hlsl"

            half4 FragmentFunc(DomainOut varyings, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                half height;
                half3 normal_ws;

                SampleScrolls(varyings.position_ws.xz, height, normal_ws);
                half height_01 = ScrollHeight_01(height);

                half3 viewDir_ws = normalize(varyings.viewVector_ws);
                half3 lightDir_ws = _MainLightPosition.xyz;

                float2 uv_ss = GetSSUV(varyings.position_ss);

                half heightMask = pow(saturate(height_01 * abs(normal_ws.y) * abs(normal_ws.y) - (1 - _FoamAmount)), _FoamPower);
                half depthMask = pow(1.0 - saturate(Linear01Depth(SampleSceneDepth(uv_ss), _ZBufferParams) * _ProjectionParams.z - (_FoamCover + varyings.position_ss.w - 1)), _FoamPower);
                half foamtex = saturate(tex2D(_FoamMask_0, varyings.position_ws.xz * _FoamMask_0_ST.xy + _Time.y * _FoamMask_0_ST.zw).r + tex2D(_FoamMask_1, varyings.position_ws.xz * _FoamMask_1_ST.xy + _Time.y * _FoamMask_1_ST.zw).r);
                half foamMask = saturate(_IsRednerFoam) * foamtex * saturate(heightMask + depthMask);
                half3 color = _Color.rgb;

                if(isFrontFace)
                {
                    half3 reflection_ws = reflect(viewDir_ws, normal_ws);
                    half roughness = max(1 - sqrt(_Specular), HALF_MIN);

                    color = SurfaceColor(color, varyings.position_ss, uv_ss, normal_ws, _Refraction, _Depth, _DepthPower, _ShallowPower);
                    half3 envColor = SAMPLE_TEXTURECUBE(unity_SpecCube0, bilinearRepeatSampler, reflection_ws).rgb;
                    half3 sssColor = sqrt(_MainLightColor.rgb * color);

                    half3 diff = lerp(saturate(Diffuse(half3(0, 1, 0), lightDir_ws)) * color, _FoamColor.rgb, foamMask);
                    half3 spec = saturate(Specular(normal_ws, viewDir_ws, lightDir_ws, roughness, _SpecCut * 1000.0) - foamMask) * _MainLightColor.rgb;

                    half3 env = saturate(Enviroment(normal_ws, viewDir_ws, _Fresnel, _Reflectivity) - foamMask) * envColor;
                    half3 sss = SSS(height_01, normal_ws, viewDir_ws, lightDir_ws, _SSSPower, _SSSIntensity, _SSSNormal, _SSSFresnel) * sssColor;

                    return half4(saturate(diff + sss + env + spec), 1);
                }
                
                normal_ws = -normal_ws;

                half contrast = (_BackContrast * _BackContrast) * 10000 + 1;
                half highlight = Fresnel(normal_ws, viewDir_ws, _BackFresnel);
                half sunY = saturate(_MainLightPosition.y);
                color = BackSurfaceColor(lerp(sqrt(color * _MainLightColor.rgb) * sqrt(sunY) * 2, _Color.rgb, highlight), uv_ss, normal_ws, viewDir_ws, _Fresnel, contrast);

                return half4(color, 1);
            }
            
            ENDHLSL
        }
    }
}
