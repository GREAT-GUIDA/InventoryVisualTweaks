sampler uImage0 : register(s0);
float uTime;
float uScrollSpeed;
float uScrollSpeed2;
float uScrollPhase;
float uSliceWidth;
float uSliceWidth2;
float uEdgeSoftness;
float uRootSoftness;
float uRootMinBrightness;
float uLayer2Strength;

float SwipeLuminance(float4 tex) {
    return tex.a * max(tex.r, max(tex.g, tex.b));
}

float4 SampleSwipe(float beamAcross, float beamAlong, float scrollSpeed, float sliceWidth) {
    float sampleU = frac(uScrollPhase + uTime * scrollSpeed + beamAcross * sliceWidth);
    return tex2D(uImage0, float2(sampleU, beamAlong));
}

float4 MainPS(float2 coords : TEXCOORD0, float4 vertColor : COLOR0) : COLOR0 {
    float beamAcross = coords.x;
    float beamAlong = coords.y;

    float edge = smoothstep(0.0, uEdgeSoftness, beamAcross)
        * smoothstep(1.0, 1.0 - uEdgeSoftness, beamAcross);

    float rootFade = lerp(
        uRootMinBrightness,
        1.0,
        smoothstep(0.0, uRootSoftness, 1.0 - beamAlong));

    float4 tex1 = SampleSwipe(beamAcross, beamAlong, uScrollSpeed, uSliceWidth);
    float4 tex2 = SampleSwipe(beamAcross, beamAlong, uScrollSpeed2, uSliceWidth2);

    float lum1 = SwipeLuminance(tex1);
    float lum2 = SwipeLuminance(tex2) * uLayer2Strength;
    float3 rgb = vertColor.rgb * (tex1.rgb * lum1 + tex2.rgb * lum2);
    float alpha = (lum1 + lum2) * edge * rootFade * vertColor.a;

    return float4(rgb, alpha);
}

technique Technique1 {
    pass P0 {
        PixelShader = compile ps_3_0 MainPS();
    }
}
