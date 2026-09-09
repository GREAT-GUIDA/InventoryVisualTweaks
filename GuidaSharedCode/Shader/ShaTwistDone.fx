



float iTime;

float4 uScreenResolution;   // xy = 画布尺寸，zw = 归一化系数
float4 iDrawingUV;
float4 iColor;
float4 iPos;
float4 iRate;


// 纹理与采样器声明
sampler uImage0 : register(s0);
texture uImage1;
sampler uImage1Sampler = sampler_state{ texture = <uImage1>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap; };


struct PSInput
{
    float2 vTexcoord : TEXCOORD0;
};

float4 mainPS(PSInput input) : COLOR0
{
    float4 icol = tex2D(uImage0, input.vTexcoord);

    float2 uv = input.vTexcoord * uScreenResolution;

    float2 off = float2(icol.x - icol.y, icol.z - icol.w);

    float r = tex2D(uImage1Sampler, (uv + off * 40.0) / uScreenResolution).r;
    float g = tex2D(uImage1Sampler, (uv + off * 30.0) / uScreenResolution).g;
    float b = tex2D(uImage1Sampler, (uv + off * 20.0) / uScreenResolution).b;

    return float4(r, g, b, 1.0);
}
technique PostTwistTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 mainPS();
    }
};