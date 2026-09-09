sampler uImage0 : register(s0);
float uIntensity;
float uProgress;
float3 uColor;
float uOpacity;

float3 HSVtoRGB(float h, float s, float v)
{
    h = frac(h / 360.0) * 6.0;

    float c = v * s;
    float hmod2 = h - floor(h / 2.0) * 2.0;
    float x = c * (1.0 - max(hmod2 - 1.0, 1.0 - hmod2));
    float m = v - c;

    float3 rgb;

    if (h < 1.0)
        rgb = float3(c, x, 0);
    else if (h < 2.0)
        rgb = float3(x, c, 0);
    else if (h < 3.0)
        rgb = float3(0, c, x);
    else if (h < 4.0)
        rgb = float3(0, x, c);
    else if (h < 5.0)
        rgb = float3(x, 0, c);
    else
        rgb = float3(c, 0, x);

    return rgb + m;
}

float3 RGBtoHSV(float3 rgb)
{
    float maxVal = max(rgb.r, max(rgb.g, rgb.b));
    float minVal = min(rgb.r, min(rgb.g, rgb.b));
    float delta = maxVal - minVal;

    float h = 0.0;
    float s = 0.0;
    float v = maxVal;

    if (maxVal > 0.00001)
        s = delta / maxVal;

    if (delta > 0.00001)
    {
        if (rgb.r == maxVal)
            h = (rgb.g - rgb.b) / delta;
        else if (rgb.g == maxVal)
            h = 2.0 + (rgb.b - rgb.r) / delta;
        else
            h = 4.0 + (rgb.r - rgb.g) / delta;

        h *= 60.0;
        if (h < 0.0)
            h += 360.0;
    }

    return float3(h, s, v);
}

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0
{
    float4 color = tex2D(uImage0, coords) * vertColor;

    if (color.a > 0.0)
    {
        float3 hsv = RGBtoHSV(color.rgb);
        color.rgb = lerp(color.rgb, HSVtoRGB(uColor.x * 360.0, uColor.y * hsv.y, uColor.z * hsv.z), uIntensity);
    }

    color *= uOpacity;
    return color;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}
