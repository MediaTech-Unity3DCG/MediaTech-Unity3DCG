// 4x3の展開図上のUV座標から、キューブマップのサンプル方向を計算する
float3 CalculateUV2TexCoord(float2 uv){
    float2 tmp  = float2(uv.x*4,uv.y*3);
    float2 mods = float2(0.0,0.0);
    float2 lps  = float2(0.0,0.0);
    mods.x = modf(tmp.x,lps.x);
    mods.y = modf(tmp.y,lps.y);
    if (lps.y == 0.0){
        if (lps.x!=1.0){
            return float3(2.0,0.0,0.0);
        }else {
            return float3(2.0*mods.x-1.0,-1.0,2.0*mods.y-1.0);
        }
    }
    if (lps.y == 1.0){
        if (lps.x == 0.0){
            return float3(-1.0,2.0*mods.y-1.0,2.0*mods.x-1.0);
        }
        if (lps.x == 1.0){
            return float3(2.0*mods.x-1.0,2.0*mods.y-1.0,1.0);
        }
        if (lps.x == 2.0){
            return float3(1.0,2.0*mods.y-1.0,-2.0*mods.x+1.0);
        }
        return float3(-2.0*mods.x+1.0,2.0*mods.y-1.0,-1.0);
    }
    if (lps.y == 2.0){
        if (lps.x!=1.0){
            return float3(2.0,0.0,0.0);
        }else{
            return float3(2.0*mods.x-1.0,+1.0,-2.0*mods.y+1.0);
        }
    }
    return float3(2.0,0.0,0.0);
}