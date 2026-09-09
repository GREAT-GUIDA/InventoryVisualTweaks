sampler uImage0 : register(s0);
float3 uColor;
float uOpacity;
float uTime;
float uRingSpeed;
float uRingCount;
float uRingWidth;
float uColorMix;

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0 {
    float4 tex = tex2D(uImage0, coords);
    float bloom = max(tex.r, max(tex.g, tex.b)) * tex.a;
    if (bloom <= 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);

    float2 centered = coords - 0.5;
    float dist = length(centered) * 2.0;
    float phase = dist * uRingCount - uTime * uRingSpeed;
    float band = abs(frac(phase) - 0.5) * 2.0;
    float ring = 1.0 - smoothstep(0.0, uRingWidth, band);

    float radialMask = bloom * smoothstep(0.0, 0.12, dist) * smoothstep(1.0, 0.5, dist);
    float alpha = ring * radialMask * uOpacity * vertColor.a;
    float3 rgb = lerp(tex.rgb, uColor, uColorMix);

    return float4(rgb, alpha);
}

technique Technique1 {
    pass P0 {
        PixelShader = compile ps_3_0 MainPS();
    }
}
