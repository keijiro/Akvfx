void K4aVertex_float(
    float2 uv,
    Texture2D positionMap,
    out float3 outPosition,
    out float3 outNormal
)
{
    uint tw, th;
    positionMap.GetDimensions(tw, th);

    int tx = uv.x * tw;
    int ty = uv.y * th;

    float3 p = positionMap.Load(int3(tx, ty, 0)).xyz;
    float3 p_x0 = positionMap.Load(int3(tx - 1, ty, 0)).xyz;
    float3 p_x1 = positionMap.Load(int3(tx + 1, ty, 0)).xyz;
    float3 p_y0 = positionMap.Load(int3(tx, ty - 1, 0)).xyz;
    float3 p_y1 = positionMap.Load(int3(tx, ty + 1, 0)).xyz;

    outPosition = p;
    outNormal = -normalize(cross(p_x1 - p_x0, p_y1 - p_y0));
}

void K4aColor_float(
    float2 uv,
    Texture2D colorMap,
    out float3 outColor,
    out float outAlpha
)
{
    uint tw, th;
    colorMap.GetDimensions(tw, th);

    int tx = uv.x * tw;
    int ty = uv.y * th;

    float4 c = colorMap.Load(int3(tx, ty, 0));
    float a_x0 = colorMap.Load(int3(tx - 1, ty - 2, 0)).a;
    float a_x1 = colorMap.Load(int3(tx + 1, ty - 2, 0)).a;
    float a_y0 = colorMap.Load(int3(tx - 1, ty + 2, 0)).a;
    float a_y1 = colorMap.Load(int3(tx + 1, ty + 2, 0)).a;

    outColor = c.rgb;
    outAlpha = min(c.a, min(min(a_x0, a_x1), min(a_y0, a_y1)));
}
