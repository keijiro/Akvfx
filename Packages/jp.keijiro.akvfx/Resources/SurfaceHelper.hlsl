void K4aVertex_float(
    float2 uv,
    Texture2D positionMap,
    out float3 outPosition,
    out float3 outNormal,
    out float3 outTangent
)
{
    uint tw, th;
    positionMap.GetDimensions(tw, th);

    int tx = uv.x * tw;
    int ty = uv.y * th;

    float4 p = positionMap.Load(int3(tx, ty, 0));
    float3 p_x0 = positionMap.Load(int3(tx - 4, ty, 0)).xyz;
    float3 p_x1 = positionMap.Load(int3(tx + 4, ty, 0)).xyz;
    float3 p_y0 = positionMap.Load(int3(tx, ty - 4, 0)).xyz;
    float3 p_y1 = positionMap.Load(int3(tx, ty + 4, 0)).xyz;

    outPosition = p.xyz;
    outNormal = -normalize(cross(p_x1 - p_x0, p_y1 - p_y0));
    outTangent = lerp(float3(1, 0, 0), float3(0, 1, 0), p.w);
}

void K4aColor_float(
    float2 uv,
    float3 tangent,
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

    outColor = c.rgb;
    outAlpha = smoothstep(0.99, 0.9999, tangent.y);
}
