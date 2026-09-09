sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float2 uVelocity;
float uWidth;

float waveTriangle(float time) {
    return abs(frac(time - 0.25) - 0.5) * 4.0 - 1.0;
}

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0{
    float4 color = tex2D(uImage0, coords) * vertColor;
    float2 center = float2(0.5, 0.5);
    float2 dir = coords - center;
    float angle = atan2(dir.y, dir.x);
    float sectorPosition = angle * uProgress / 6.28318530;
    float sectorIndicator = waveTriangle(sectorPosition + uTime) + uWidth;
    float rate = step(0.0, sectorIndicator);
    color *= rate;
    return color;
}

technique Technique1{
    pass P0{
        PixelShader = compile ps_2_0 MainPS();
    }
}
