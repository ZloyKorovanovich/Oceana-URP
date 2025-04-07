#ifndef _MAP_PACKING_INCLUDED
#define _MAP_PACKING_INCLUDED

inline float3 PackRGBNormal(float3 normal) {
    return (normal + 1) * 0.5;
}

inline float3 UnpackRGBNormal(float3 sample) {
    return sample.rgb * 2 - 1;
}


inline float2 PackRGUpNormal(float3 normal) {
    return (normal.xz + 1) * 0.5;
}

inline float3 UnpackRGUpNormal(float2 sample) {
    float3 normal = float3(sample.r * 2 - 1, 0, sample.g * 2 - 1);
    normal.y = sqrt(1 - normal.x * normal.x + normal.z * normal.z);
    return normal;
}

#endif