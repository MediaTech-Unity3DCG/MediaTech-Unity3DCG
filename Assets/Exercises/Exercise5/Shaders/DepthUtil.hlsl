float Linear01DepthFromViewZ(float viewZ, float nearClip, float farClip)
{
#if defined(UNITY_REVERSED_Z)
	return (viewZ - farClip) / (nearClip - farClip);
#else
	return (viewZ - nearClip) / (farClip - nearClip);
#endif
}
float ViewZFromLinear01Depth(float linear01Depth, float nearClip, float farClip)
{
#if defined(UNITY_REVERSED_Z)
	return linear01Depth * (nearClip - farClip) + farClip;
#else
	return linear01Depth * (farClip - nearClip) + nearClip;
#endif
}