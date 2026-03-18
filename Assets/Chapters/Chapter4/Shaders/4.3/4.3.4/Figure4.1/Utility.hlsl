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

