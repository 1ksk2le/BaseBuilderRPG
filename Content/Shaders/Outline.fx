#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 texelSize = float2(0.0f, 0.0f);
float4 outlineColor = float4(1.0f, 1.0f, 1.0f, 1.0f);

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 pos = float2(input.TextureCoordinates.x, input.TextureCoordinates.y);
    float4 col = tex2D(SpriteTextureSampler, pos) * input.Color;
    
    float center = ceil(col.a);

    // Adjust the sampling positions to extend beyond the sprite
    float2 leftPos = float2(input.TextureCoordinates.x - 1.0f * texelSize.x, input.TextureCoordinates.y);
    float left = ceil(tex2D(SpriteTextureSampler, leftPos).a) * ceil(leftPos.x);

    float2 rightPos = float2(input.TextureCoordinates.x + 1.0f * texelSize.x, input.TextureCoordinates.y);
    float right = ceil(tex2D(SpriteTextureSampler, rightPos).a) * 1.0f - floor(rightPos.x);

    float2 upPos = float2(input.TextureCoordinates.x, input.TextureCoordinates.y - 1.0f * texelSize.y);
    float up = ceil(tex2D(SpriteTextureSampler, upPos).a) * ceil(upPos.y);

    float2 downPos = float2(input.TextureCoordinates.x, input.TextureCoordinates.y + 1.0f * texelSize.y);
    float down = ceil(tex2D(SpriteTextureSampler, downPos).a) * 1.0f - floor(downPos.y);

    float total = (left + right + up + down);
    if (center > 0.0f && total < 4.0f)
        col = outlineColor;
    return col;
}


technique OutlineEffect
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};