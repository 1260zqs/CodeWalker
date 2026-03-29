struct VS_IN
{
    float2 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct PS_IN
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 worldPosition : TEXCOORD1;
};

cbuffer Draw : register(b0)
{
    matrix _ObjectToWorld;
    matrix _ViewProjection;
    float4 _ClipRect;
};

PS_IN main(VS_IN input)
{
    PS_IN o;
    o.worldPosition = mul(_ObjectToWorld, float4(input.vertex, 0, 1.0));
    o.vertex = mul(_ViewProjection, o.worldPosition);
    o.uv = input.uv;
    return o;
}
