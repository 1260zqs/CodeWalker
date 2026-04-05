struct VertexOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
};

Texture2D _MainTex : register(t0);
SamplerState _MainTex_Sampler : register(s0);

float4 main(VertexOutput i) : SV_Target
{
    return _MainTex.Sample(_MainTex_Sampler, i.uv);
}