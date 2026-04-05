struct VertexInput
{
    float2 position : POSITION;
    float2 uv : TEXCOORD0;
};

struct VertexOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
};

VertexOutput main(VertexInput v)
{
    VertexOutput o;
    o.position = float4(v.position, 0, 1);
    o.uv = v.uv;
    return o;
}