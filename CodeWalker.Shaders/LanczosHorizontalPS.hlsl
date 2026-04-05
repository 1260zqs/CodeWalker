struct VertexOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
};

cbuffer DownscaleParams : register(b0)
{
    float2 SourceSize;
    float2 TargetSize;
    float ScaleX;
    float ScaleY;
};

Texture2D _MainTex : register(t0);
SamplerState _MainTex_Sampler : register(s0);

float Lanczos(float x)
{
    x = abs(x);
    if (x < 0.00001f) return 1.0f;
    if (x >= 3.0f) return 0.0f;

    float pix = 3.14159265359f * x;
    return (sin(pix) / pix) * (sin(pix / 3.0f) / (pix / 3.0f));
}

float4 main(VertexOutput i) : SV_Target
{
    float2 srcPixel = i.uv * SourceSize;
    float scale = max(ScaleX, 0.00001f);

    float4 color = 0.0f;
    float weightSum = 0.0f;
    for (int k = -3; k <= 3; k++)
    {
        float sampleX = srcPixel.x + k;
        float2 sampleUV = float2(sampleX / SourceSize.x, i.uv.y);
        float4 s = _MainTex.Sample(_MainTex_Sampler, sampleUV);
        float w = Lanczos(k * scale);
        color += s * w;
        weightSum += w;
    }

    return color / weightSum;
}
