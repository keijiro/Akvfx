Shader "Hidden/Akvfx/Unproject"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    texture2D _ColorTexture;
    texture2D _DepthTexture;
    StructuredBuffer<float> _XYTable;
    float _MaxDepth;

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
        uint w, h;
        _ColorTexture.GetDimensions(w, h);

        // Texture index
        uint tx = texCoord.x * w;
        uint ty = texCoord.y * h;

        // Color sample
        float4 color = _ColorTexture[uint2(tx, ty)];

        // Depth sample (int16 -> float)
        int d0 = _DepthTexture[uint2(tx * 2 + 0, ty)] * 255;
        int d1 = _DepthTexture[uint2(tx * 2 + 1, ty)] * 255;
        float depth = (float)(d0 + (d1 << 8)) / 1000;
        float mask = depth > 0 && depth < _MaxDepth;
        float z = lerp(_MaxDepth, depth, mask);

        // XY table lookup
        uint xy_i = (tx + ty * w) * 2;
        float2 xy = float2(_XYTable[xy_i], -_XYTable[xy_i + 1]);

        // MRT output write
        colorOut = float4(color.rgb, mask);
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
