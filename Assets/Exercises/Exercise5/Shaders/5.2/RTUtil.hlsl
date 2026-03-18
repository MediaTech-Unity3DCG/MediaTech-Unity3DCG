
#include "UnityRayTracingMeshUtils.cginc"
#include "UnityShaderVariables.cginc"

float2 triangleInterpolation2(float2 value_0, float2 value_1, float2 value_2, float2 uv)
{
    return value_0 + (value_1 - value_0) * uv.x + (value_2 - value_0) * uv.y;
}
float3 triangleInterpolation3(float3 value_0, float3 value_1, float3 value_2, float2 uv)
{
    return value_0 + (value_1 - value_0) * uv.x + (value_2 - value_0) * uv.y;
}
float4 triangleInterpolation4(float4 value_0, float4 value_1, float4 value_2, float2 uv)
{
    return value_0 + (value_1 - value_0) * uv.x + (value_2 - value_0) * uv.y;
}

float3 transformObjectToWorldPosition(float3 position)
{
    return mul(ObjectToWorld3x4(), float4(position, 1));
}
float3 transformObjectToWorldNormal(float3 normal)
{
    return normalize(mul(ObjectToWorld3x4(), float4(normal, 0)));
}
float3 transformObjectToWorldDirection(float3 direction)
{
    return mul(ObjectToWorld3x4(), float4(direction, 0));
}

float3 transformWorldToObjectPosition(float3 position)
{
    return mul(WorldToObject3x4(), float4(position, 1));
}
float3 transformWorldToObjectNormal(float3 normal)
{
    return normalize(mul(WorldToObject3x4(), float4(normal, 0)));
}
float3 transformWorldToObjectDirection(float3 direction)
{
    return mul(WorldToObject3x4(), float4(direction, 0));
}

struct SurfaceData {
    float3 vNormal;
    float3 fNormal;
    float2 uv;
	float4 vTangent;
};;

RayDesc     GenerateRay(float2 screenPos, float2 screenDimensions)
{
    float2  ndcCoords     = (screenPos / screenDimensions) * 2 - 1;
    ndcCoords             = ndcCoords / unity_CameraProjection._m11;// fovを打ち消す
    float   aspectRatio   = (float)screenDimensions.x / (float)screenDimensions.y;
    float3  viewDirection = normalize(float3(ndcCoords.x * aspectRatio, ndcCoords.y, 1));
    float3  rayDirection  = normalize(mul((float3x3)unity_CameraToWorld, viewDirection));
    float3  rayOrigin     = _WorldSpaceCameraPos;
    RayDesc ray           = { rayOrigin,0.01,rayDirection,1000.0 };
    return  ray;
}
SurfaceData GetSurfaceData(float2 bary)
{
    uint   primitiveIndex = PrimitiveIndex();
    uint3  indices = UnityRayTracingFetchTriangleIndices(primitiveIndex);
    float3 vpositions[] = {
        UnityRayTracingFetchVertexAttribute3(indices.x,kVertexAttributePosition),
        UnityRayTracingFetchVertexAttribute3(indices.y,kVertexAttributePosition),
        UnityRayTracingFetchVertexAttribute3(indices.z,kVertexAttributePosition),
    };
    float3 fnormal = normalize(transformObjectToWorldNormal(cross(vpositions[1] - vpositions[0], vpositions[2] - vpositions[0])));
    float3 vnormal = normalize(transformObjectToWorldNormal(triangleInterpolation3(
        UnityRayTracingFetchVertexAttribute3(indices.x, kVertexAttributeNormal),
        UnityRayTracingFetchVertexAttribute3(indices.y, kVertexAttributeNormal),
        UnityRayTracingFetchVertexAttribute3(indices.z, kVertexAttributeNormal), bary
    )));
    float4 vTangent0 = UnityRayTracingFetchVertexAttribute4(indices.x, kVertexAttributeTangent);
    float4 vTangent1 = UnityRayTracingFetchVertexAttribute4(indices.y, kVertexAttributeTangent);
    float4 vTangent2 = UnityRayTracingFetchVertexAttribute4(indices.z, kVertexAttributeTangent);
    float3 vTangent = normalize(transformObjectToWorldNormal(triangleInterpolation3(
        vTangent0.xyz,
        vTangent1.xyz,
        vTangent2.xyz, bary
    )));
    float3 vBitangent = normalize(cross(vnormal, vTangent));
    vTangent = normalize(cross(vBitangent, vnormal));
    float2 uv = triangleInterpolation2(
        UnityRayTracingFetchVertexAttribute2(indices.x, kVertexAttributeTexCoord0),
        UnityRayTracingFetchVertexAttribute2(indices.y, kVertexAttributeTexCoord0),
        UnityRayTracingFetchVertexAttribute2(indices.z, kVertexAttributeTexCoord0), bary
    );
    SurfaceData surfaceData = { vnormal,fnormal,uv, float4(vTangent.x,vTangent.y,vTangent.z,vTangent0.w) };
    return surfaceData;
}