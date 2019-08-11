Shader "Hidden/Akvfx/Short3ToFloat3"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    texture2D _SourceTexture;
    float2 _Dimensions;

    void Vertex(
        float4 position : POSITION,
        out float4 positionOut : SV_Position,
        inout float2 texCoord : TEXCOORD0
    )
    {
        positionOut = UnityObjectToClipPos(position);
    }

    float4 Fragment(
        float4 position : SV_Position,
        float2 texCoord : TEXCOORD0
    ) : SV_Target
    {
        uint sx = texCoord.x * _Dimensions.x;
        uint sy = texCoord.y * _Dimensions.y;

        sx *= 6;

        int b0 = _SourceTexture[uint2(sx + 0, sy)] * 255;
        int b1 = _SourceTexture[uint2(sx + 1, sy)] * 255;
        int b2 = _SourceTexture[uint2(sx + 2, sy)] * 255;
        int b3 = _SourceTexture[uint2(sx + 3, sy)] * 255;
        int b4 = _SourceTexture[uint2(sx + 4, sy)] * 255;
        int b5 = _SourceTexture[uint2(sx + 5, sy)] * 255;

        float3 xyz = float3(b0 + (b1 << 8), b2 + (b3 << 8), b4 + (b5 << 8));
        xyz = (xyz < 0x8000 ? xyz : xyz - 0x10000) / 0x8000;

        return float4(xyz, 1);
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
