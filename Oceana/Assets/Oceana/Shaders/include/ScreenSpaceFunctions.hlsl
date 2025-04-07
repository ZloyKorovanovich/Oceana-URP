#ifndef _SCREEN_SPACE_FUNCTIONS_INCLUDED
#define _SCREEN_SPACE_FUNCTIONS_INCLUDED

inline float4 TransformClipToScreen(float4 positionCS, float4 projectionParams) {
    float4 o = positionCS * 0.5f;
    o.xy = float2(o.x, o.y * projectionParams.x) + o.w;
    o.zw = positionCS.zw;
    return o;
}

inline float2 TransformScreenToUV(float4 screenPosition) {
    return screenPosition.xy / screenPosition.w;
}

inline float LinearEyeDepth(float z, float4 zBufferParams) {
    return 1.0 / (zBufferParams.z * z + zBufferParams.w);
}

inline float Linear01Depth(float depth, float4 zBufferParams) {
    return 1.0 / (zBufferParams.x * depth + zBufferParams.y);
}

inline float2 RefractUV(float2 uv, float strength, float3 normal) {
    return uv + (normal.xz * 2 - 1) * strength;
}

#endif