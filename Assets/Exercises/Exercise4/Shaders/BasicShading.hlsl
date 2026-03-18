#include "UnityCG.cginc"
UNITY_DECLARE_TEXCUBE(_Envmap);
float3   sampleEnvmapLod(float3 v,float lod) {
    float4 col = UNITY_SAMPLE_TEXCUBE_LOD(_Envmap, v, lod);
    return col.xyz;
}
int2     getEnvmapTexelSize() {
	int width; int height;;
	_Envmap.GetDimensions(width, height);
	return int2(width, height);
}
struct   appdata{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};
struct   v2f {
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
    float3 worldPos : POSITION1;
    float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};
v2f      vertDefault(appdata v){
    v2f o;
    o.vertex   = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.normal   = UnityObjectToWorldNormal(v.normal);
    o.uv       = v.uv;
    return o;
}
float4   vertScreen(float4 v : POSITION) : SV_POSITION{
    return UnityObjectToClipPos(v);
}
float3x3 calcONB(float3 n) {
    const float iSq3 = 0.57735026919;
    float3 vup = float3(0.0, 0.0, 0.0);
    if (abs(n.x)<iSq3){vup.x=1.0;}
    else if(abs(n.y)<iSq3){vup.y=1.0;}
    else {vup.z=1.0;}
    float3 t = normalize(cross(vup, n));
    float3 b = cross(n, t);
    return float3x3(t, b, n);
}
float3   uniformHemiSphere(float2 v){
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = v.y;
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
float3   uniformHemiCosine(float2 v, float s){
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = pow(v.y, 1.0/(s+1.0));
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
float3  uniformHemiCosine(float2 v) {
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = sqrt(v.y);
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
float3   uniformGGXCosine(float2 r, float alpha) {
    float tanTht = alpha * sqrt(r.x / (1.0 - r.x));
    float phi = 2.0 * UNITY_PI * r.y;
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    float x = tanTht * cosPhi;
    float y = tanTht * sinPhi;
    float z = 1;
    return normalize(float3(x, y, z));
}
float  GGX_D(float3 h, float alpha) {
    float nh = h.z;
    float nh2 = nh * nh; float alpha2 = alpha * alpha;
    return alpha2 / (UNITY_PI * pow(nh2 * (alpha2 - 1) + 1, 2));
}
float  GGX_G1(float3 v, float3 h, float alpha) {
    float nv = v.z; float alpha2 = alpha * alpha;
    return 2 * nv / (nv + sqrt(alpha2 + (1 - alpha2) * nv * nv));
}
float  GGX_G2(float3 i, float3 r, float3 h, float alpha) {
    return GGX_G1(i, h, alpha) * GGX_G1(r, h, alpha);
}
float3 GGX_F(float3 i, float3 h, float3 F0) {
    return F0 + (1 - F0) * pow(1 - dot(h, i), 5);
}
float  GGX_F(float3 i, float3 h, float F0) {
    return F0 + (1 - F0) * pow(1 - dot(h, i), 5);
}
float3   calcCubePosition(float2 v, uint face) {
    v = v * 2.0 - 1.0;
    if (face == 0) return float3(1.0, -v.y, -v.x);
    if (face == 1) return float3(-1.0, -v.y, v.x);
    if (face == 2) return float3(v.x, +1.0, v.y);
    if (face == 3) return float3(v.x, -1.0, -v.y);
    if (face == 4) return float3(v.x, -v.y, 1.0);
    /* if (face == 5)*/ return float3(-v.x, -v.y, -1.0);
}
float Disney_F(float f90, float cosine) {
	float oneMinusCos = max(1.0 - cosine, 0.0);
	return 1.0 + (f90 - 1.0) * pow(oneMinusCos, 5);
}
float Disney_F90(float3 i, float3 h, float roughness) {
	float iDotH = dot(i, h);
	return 0.5 + 2.0 * roughness * iDotH * iDotH;
}
