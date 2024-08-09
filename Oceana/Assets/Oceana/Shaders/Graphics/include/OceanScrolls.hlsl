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

sampler2D _Scroll_0, _Scroll_1, _Scroll_2, _Scroll_3;
float4 _Scroll_0_ST, _Scroll_1_ST, _Scroll_2_ST, _Scroll_3_ST;
half4 _ScrollHeights;

// Normals
half3 ConstructNormal(half3 packed)
{
    return packed * 2 - half3(1, 1, 1);
}

half3 NormalStrength(half3 normal, half strength)
{
    normal.xz *= strength;
    return normalize(normal);
}

// Functions
half SampleScrollsHeight_WS(float2 uv)
{
    half h_0 = tex2Dlod(_Scroll_0, float4(uv * _Scroll_0_ST.xy + _Scroll_0_ST.zw, 0, 0)).a * _ScrollHeights.x;
    half h_1 = tex2Dlod(_Scroll_1, float4(uv * _Scroll_1_ST.xy + _Scroll_1_ST.zw, 0, 0)).a * _ScrollHeights.y;
    half h_2 = tex2Dlod(_Scroll_2, float4(uv * _Scroll_2_ST.xy + _Scroll_2_ST.zw, 0, 0)).a * _ScrollHeights.z;
    half h_3 = tex2Dlod(_Scroll_3, float4(uv * _Scroll_3_ST.xy + _Scroll_3_ST.zw, 0, 0)).a * _ScrollHeights.w;

    return (h_0 + h_1 + h_2 + h_3) * 0.25;
}

half3 SampleScrollsNormal_WS(float2 uv)
{
    half3 n_0 = NormalStrength(ConstructNormal(tex2D(_Scroll_0, uv * _Scroll_0_ST.xy + _Scroll_0_ST.zw).rgb), _ScrollHeights.x);
    half3 n_1 = NormalStrength(ConstructNormal(tex2D(_Scroll_1, uv * _Scroll_1_ST.xy + _Scroll_1_ST.zw).rgb), _ScrollHeights.y);
    half3 n_2 = NormalStrength(ConstructNormal(tex2D(_Scroll_2, uv * _Scroll_2_ST.xy + _Scroll_2_ST.zw).rgb), _ScrollHeights.z);
    half3 n_3 = NormalStrength(ConstructNormal(tex2D(_Scroll_3, uv * _Scroll_3_ST.xy + _Scroll_3_ST.zw).rgb), _ScrollHeights.w);

    return NormalStrength(normalize(n_0 + n_1 + n_2 + n_3), 0.25);
}

void SampleScrolls(float2 uv, out half height_ws, out half3 normal_ws)
{
    half4 s_0 = tex2D(_Scroll_0, uv * _Scroll_0_ST.xy + _Scroll_0_ST.zw);
    half4 s_1 = tex2D(_Scroll_1, uv * _Scroll_1_ST.xy + _Scroll_1_ST.zw);
    half4 s_2 = tex2D(_Scroll_2, uv * _Scroll_2_ST.xy + _Scroll_2_ST.zw);
    half4 s_3 = tex2D(_Scroll_3, uv * _Scroll_3_ST.xy + _Scroll_3_ST.zw);

    half3 n_0 = NormalStrength(ConstructNormal(s_0.rgb), _ScrollHeights.x);
    half3 n_1 = NormalStrength(ConstructNormal(s_1.rgb), _ScrollHeights.y);
    half3 n_2 = NormalStrength(ConstructNormal(s_2.rgb), _ScrollHeights.z);
    half3 n_3 = NormalStrength(ConstructNormal(s_3.rgb), _ScrollHeights.w);

    half h_0 = s_0.a * _ScrollHeights.x;
    half h_1 = s_1.a * _ScrollHeights.y;
    half h_2 = s_2.a * _ScrollHeights.z;
    half h_3 = s_3.a * _ScrollHeights.w;

    height_ws = (h_0 + h_1 + h_2 + h_3) * 0.25;
    normal_ws = NormalStrength(normalize(n_0 + n_1 + n_2 + n_3), 0.25);
}

half ScrollHeight_01(half height)
{
    return height * 4 / (_ScrollHeights.x + _ScrollHeights.y + _ScrollHeights.z + _ScrollHeights.w);
}