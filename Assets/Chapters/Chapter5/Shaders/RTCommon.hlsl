#include "RTUtil.hlsl"
struct RayPayloadData {
	float3 diffuseColor;// 拡散色
	float3 lightColor;// 光源の発光色
	float3 hitPoint;// 交差点の位置
	float3 hitNormal;// 交差点の法線
	float4 hitTangent;// 交差点の接線
	bool isDone;// レイの処理が完了したか
};
struct RayAttributeData { float2 barycentrics; };