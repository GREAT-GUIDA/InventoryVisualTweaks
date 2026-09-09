sampler uImage0 : register(s0);
float uFrequency;
float uRotation;
float3 uColor;
float uOpacity;
float uTime;
float uWidth;
float2 uImageSize0;
float4 uSourceRect;

float waveTriangle(float time) {
    return abs(frac(time - 0.25) - 0.5) * 4.0 - 1.0;
}

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0{
    float4 color = tex2D(uImage0, coords) * vertColor;
    if (color.a > 0.0) {
        color.rgb = uColor;
        color.a = uOpacity;

        float2 texcoord = coords * uImageSize0;
        float2 uv = (texcoord - uSourceRect.xy) / (uSourceRect.zw - uSourceRect.xy) - float2(0.5f, 0.5f);
        float progress = (uv.x * sin(uRotation) + uv.y * cos(uRotation)) * uFrequency;

        float sectorIndicator = waveTriangle(progress + uTime) + uWidth;
        float rate = smoothstep(0.0, 0.01, sectorIndicator);

        color *= rate;
    }
    return color;
}


technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}
