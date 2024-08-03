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

float3 ProjectOnPlane(float3 vec, float3 normal)
{
    return vec - normal * dot( vec, normal );
}

float3 GetViewDir_WS(float4 position_cs)
{
	float3x3 inverseView = (float3x3)UNITY_MATRIX_I_V;
	float4x4 inverseProj = UNITY_MATRIX_I_P;
	float4 viewDirectionEyeSpace = mul(inverseProj, position_cs);

	return mul(inverseView, viewDirectionEyeSpace.xyz).xyz;
}

float3 GetPosition_WS(float depth_linearEye, float3 viewDir_ws)
{
	float3 cameraForward =  -UNITY_MATRIX_V[2].xyz;
    float cameraDistance = depth_linearEye / dot(viewDir_ws, cameraForward);

    return viewDir_ws * cameraDistance + _WorldSpaceCameraPos;
}