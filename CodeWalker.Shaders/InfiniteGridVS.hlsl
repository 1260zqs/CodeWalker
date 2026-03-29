struct VSInput
{
    float2 Position : POSITION;
    float2 UV       : TEXCOORD0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 ScreenUV : TEXCOORD0;
};

PSInput main(VSInput input)
{
    PSInput output;
    output.Position = float4(input.Position, 0, 1.0f);
    output.ScreenUV = input.UV;
    return output;
}