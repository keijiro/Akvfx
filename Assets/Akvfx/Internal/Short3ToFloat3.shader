Shader "Hidden/Akvfx/Short3ToFloat3"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    texture2D _ColorTexture;
    texture2D _DepthTexture;
    float2 _Dimensions;
    StructuredBuffer<float> _XYTable;

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
        uint sx = texCoord.x * _Dimensions.x;
        uint sy = texCoord.y * _Dimensions.y;

        float4 color = _ColorTexture[uint2(sx, sy)];

        int d0 = _DepthTexture[uint2(sx * 2 + 0, sy)] * 255;
        int d1 = _DepthTexture[uint2(sx * 2 + 1, sy)] * 255;
        float depth = (float)(d0 + (d1 << 8)) / 0x8000;

        uint xy_i = (sx + sy * _Dimensions.x) * 2;
        float2 xy = float2(_XYTable[xy_i], _XYTable[xy_i + 1]);

        colorOut = color;
        positionOut = float4(xy * depth, depth, 1);
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
