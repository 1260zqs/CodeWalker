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

Texture2D _MainTex : register(t0);
SamplerState _MainTex_Sampler : register(s0);

PS_IN VSMain(VS_IN input)
{
    PS_IN o;
	o.worldPosition = mul(_ObjectToWorld, float4(input.vertex, 0, 1.0));
    o.vertex = mul(_ViewProjection, o.worldPosition);
    o.uv = input.uv;
    return o;
}

inline float Get2DClipping(in float2 position, in float4 clipRect)
{
    float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
    return inside.x * inside.y;
}

float4 PSMain(PS_IN input) : SV_Target
{
    float4 col = _MainTex.Sample(_MainTex_Sampler, input.uv);
	col.a *= Get2DClipping(input.worldPosition.xy, _ClipRect);
	clip(col.a - 0.001);
	return col;
}
