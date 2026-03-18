#include "UnityCG.cginc"
UNITY_DECLARE_TEXCUBE(_Envmap);
float3   sampleEnvmapLod(float3 v, float lod) {
    float4 col = UNITY_SAMPLE_TEXCUBE_LOD(_Envmap, v, lod);
    return col.xyz;
}
int2     getEnvmapTexelSize() {
    int width; int height;;
    _Envmap.GetDimensions(width, height);
    return int2(width, height);
}
struct appdata{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};
struct v2f {
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
    float3 worldPos : POSITION1;
    float3 normal : NORMAL;
};
v2f vertDefault(appdata v){
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    return o;
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
float3 uniformHemiSphere(float2 v){
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = v.y;
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}
float3 uniformHemiCosine(float2 v, float s){
    float phi = 2.0 * UNITY_PI * v.x;
    float cosTht = pow(v.y, 1.0/(s+1.0));
    float sinTht = sqrt(max(1.0 - cosTht * cosTht, 0.0));
    float sinPhi; float cosPhi;
    sincos(phi, sinPhi, cosPhi);
    return float3(sinTht * cosPhi, sinTht * sinPhi, cosTht);
}