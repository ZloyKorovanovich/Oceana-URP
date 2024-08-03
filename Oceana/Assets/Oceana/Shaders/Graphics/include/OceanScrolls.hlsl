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

sampler2D _Scroll_0, _Scroll_1, _Scroll_2;
float4 _Scroll_0_ST, _Scroll_1_ST, _Scroll_2_ST;
half _Height_0, _Height_1, _Height_2;

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
    half h_0 = tex2Dlod(_Scroll_0, float4(uv * _Scroll_0_ST.xy + _Scroll_0_ST.zw, 0, 0)).a * _Height_0;
    half h_1 = tex2Dlod(_Scroll_1, float4(uv * _Scroll_1_ST.xy + _Scroll_1_ST.zw, 0, 0)).a * _Height_1;
    half h_2 = tex2Dlod(_Scroll_2, float4(uv * _Scroll_2_ST.xy + _Scroll_2_ST.zw, 0, 0)).a * _Height_2;

    return (h_0 + h_1 + h_2) / 3.0;
}

void SampleScrolls(float2 uv, out half height_ws, out half3 normal_ws)
{
    half4 s_0 = tex2D(_Scroll_0, uv * _Scroll_0_ST.xy + _Scroll_0_ST.zw);
    half4 s_1 = tex2D(_Scroll_1, uv * _Scroll_1_ST.xy + _Scroll_1_ST.zw);
    half4 s_2 = tex2D(_Scroll_2, uv * _Scroll_2_ST.xy + _Scroll_2_ST.zw);

    half3 n_0 = NormalStrength(ConstructNormal(s_0.rgb), _Height_0);
    half3 n_1 = NormalStrength(ConstructNormal(s_1.rgb), _Height_1);
    half3 n_2 = NormalStrength(ConstructNormal(s_2.rgb), _Height_2);

    height_ws = (s_0.a * _Height_0 + s_1.a * _Height_1 + s_2.a * _Height_2) / 3.0;
    normal_ws = NormalStrength(normalize(n_0 + n_1 + n_2), 0.33333);
}

half ScrollHeight_01(half height)
{
    return height * 3.0 / (_Height_0 + _Height_1 + _Height_2);
}