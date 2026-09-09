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

float4x4 MatrixTransform;

float uBloomIntensity;
float uLerpIntensity;
float3 uLerpColor;
float uRadialBlurIntensity;
float2 uRadialBlurPosition;
const int nsamples = 5;

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0{
    float2 uv = coords;

    float4 sum = float4(0,0,0,0);
    float2 texel = 1. / uImageSize1 * 4.;
    float offsets[9] = { -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0 };
    float weights[9] = { 0.05, 0.09, 0.12, 0.15, 0.16, 0.15, 0.12, 0.09, 0.05 };

    for (int i = 0; i < 9; ++i){
        sum += tex2D(uImage0, uv + float2(offsets[i], 0) * texel) * weights[i];
    }
    for (int j = 0; j < 9; ++j){
        sum += tex2D(uImage0, uv + float2(0, offsets[j]) * texel) * weights[j];
    }

    float blurStart = 1.0;
    float blurWidth = 0.05 * uRadialBlurIntensity;

    uv -= uRadialBlurPosition;

    float precompute = blurWidth * (1.0 / float(nsamples - 1));

    float4 color = float4(0.0, 0.0, 0.0, 0.0);
    for (int k = 0; k < nsamples; k++)
    {
        float scale = blurStart + (float(k) * precompute);
        color += tex2D(uImage0, uv * scale + uRadialBlurPosition);
    }

    color /= float(nsamples);

    float4 original = color + sum * uBloomIntensity;

    original = lerp(original, float4(uLerpColor, 1.0), uLerpIntensity);
    return original;
}

technique Technique1{
    pass P0{
        PixelShader = compile ps_3_0 MainPS();
    }
}

