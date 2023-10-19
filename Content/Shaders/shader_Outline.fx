sampler2D TextureSampler : register(s0);

float4 OutlineColor : register(c0);
float OutlineThickness : register(c1); // The thickness of the outline

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pixelSize = OutlineThickness / float2(texCoord.x, texCoord.y);
    float4 borderColor = tex2D(TextureSampler, texCoord);

    // Determine if the pixel is part of the sprite
    bool isInteriorPixel = borderColor.a > 0.0;

    // Apply the outline to the sprite
    if (!isInteriorPixel)
    {
        float4 sample = tex2D(TextureSampler, texCoord);
        
        // Check if any of the neighboring pixels is part of the sprite
        bool isNearInteriorPixel = sample.a > 0.0;

        if (isNearInteriorPixel)
        {
            borderColor = OutlineColor;
        }
    }

    return borderColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}
