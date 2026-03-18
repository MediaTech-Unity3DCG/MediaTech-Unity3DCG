#ifndef RANDOM_HLSL
#define RANDOM_HLSL
#include "UnityCG.cginc"
uint xorshit(uint input)
{
    input ^= (input << 13u);
    input ^= (input >> 17u);
    input ^= (input << 5u);
    return input;
}
uint pcg_hash(uint input)
{
    uint state = input * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}
float UniformUintToFloat(uint u) {
    // IEEE-754: 2^-32 = 0x2F800000
    return float(u) * asfloat(0x2F800000u);
}

static uint g_randomState = 0u;
void initRandom(uint seed) {
    g_randomState = pcg_hash(seed);
}

uint Random1u() {
    g_randomState = xorshit(g_randomState);
    return g_randomState;
}

float Random1f() {
    return UniformUintToFloat(Random1u());
}

float2 Random2f() {
    return float2(Random1f(), Random1f());
}

float3 Random3f() {
    return float3(Random1f(), Random1f(), Random1f());
}
float3x3 calcONB(float3 n) {
    const float iSq3 = 0.57735026919;
    float3 vup = float3(0.0, 0.0, 0.0);
    if (abs(n.x) < iSq3) { vup.x = 1.0; }
    else if (abs(n.y) < iSq3) { vup.y = 1.0; }
    else { vup.z = 1.0; }
    float3 t = normalize(cross(vup, n));
    float3 b = cross(n, t);
    return float3x3(t, b, n);
}
float3 uniformHemiSphere(float2 v) {
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = v.y;
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
float3 uniformHemiCosine(float2 v, float s) {
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = pow(v.y, 1.0 / (s + 1.0));
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
#endif