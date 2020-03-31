Shader "Hidden/Akvfx/Unproject"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Buffer<uint> _ColorBuffer;
    Buffer<uint> _DepthBuffer;
    Buffer<float> _XYTable;
    float _MaxDepth;
    float _Width;
    float _Height;

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
        float4 vertex : POSITION,
        float2 uv : TEXCOORD0,
        out float4 outVertex : SV_Position,
        inout float2 outUV : TEXCOORD0
    )
    {
        outVertex = UnityObjectToClipPos(vertex);
        outUV = uv;
    }

    void Fragment(
        float4 vertex : SV_Position,
        float2 uv : TEXCOORD0,
        out float4 outColor : SV_Target0,
        out float4 outPosition : SV_Target1
    )
    {
        // Buffer index
        uint idx = (uint)(uv.x * _Width) + (uint)((1 - uv.y) * _Height) * _Width;

        // Color sample
        float3 color = GammaToLinearSpace(uint_to_float3(_ColorBuffer[idx]));

        // Depth sample (int16 -> float)
        float depth = uint_to_ushort(_DepthBuffer[idx >> 1], idx & 1) / 1000.0;
        float mask = depth > 0 && depth < _MaxDepth;
        float z = lerp(_MaxDepth, depth, mask);

        // XY table lookup
        float2 xy = float2(_XYTable[idx * 2], -_XYTable[idx * 2 + 1]);

        // MRT output write
        outColor = float4(color, mask);
        outPosition = float4(xy * z, z, mask);
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
