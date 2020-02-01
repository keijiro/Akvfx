Shader "Hidden/Akvfx/Unproject"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Buffer<uint> _ColorBuffer;
    Buffer<uint> _DepthBuffer;
    Buffer<float> _XYTable;
    float _MaxDepth;

    float3 uint_to_float3(uint raw)
    {
        return (uint3(raw >> 16, raw >> 8, raw) & 0xff) / 255.0;
    }

    uint uint_to_ushort(uint raw, bool high)
    {
        uint4 c4 = uint4(raw, raw >> 8, raw >> 16, raw >> 24) & 0xff;
        uint2 c2 = high ? c4.zw : c4.xy;
        return c2.x + (c2.y << 8);
    }

    void Vertex(
        float4 position : POSITION,
        out float4 positionOut : SV_Position,
        inout float2 texCoord : TEXCOORD0
    )
    {
        positionOut = UnityObjectToClipPos(position);
    }

    void Fragment(
        float4 position : SV_Position,
        float2 texCoord : TEXCOORD0,
        out float4 colorOut : SV_Target0,
        out float4 positionOut : SV_Target1
    )
    {
        // Buffer index
        uint idx = (uint)(texCoord.x * 2048) + (uint)(texCoord.y * 1536) * 2048;

        // Color sample
        float3 color = GammaToLinearSpace(uint_to_float3(_ColorBuffer[idx]));

        // Depth sample (int16 -> float)
        float depth = uint_to_ushort(_DepthBuffer[idx >> 1], idx & 1) / 1000.0;
        float mask = depth > 0 && depth < _MaxDepth;
        float z = lerp(_MaxDepth, depth, mask);

        // XY table lookup
        float2 xy = float2(_XYTable[idx * 2], -_XYTable[idx * 2 + 1]);

        // MRT output write
        colorOut = float4(color, mask);
        positionOut = float4(xy * z, z, mask);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
