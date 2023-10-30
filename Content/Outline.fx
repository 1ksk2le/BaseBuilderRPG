// Define the input and output structures
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

// Texture sampler state
sampler TextureSampler : register(s0);

// Effect parameters
float4 RarityColor : COLOR <string ObjectSemantic = "RarityColor";>;
float BorderSize : float <string ObjectSemantic = "BorderSize";>;
float4 BorderColor : COLOR <string ObjectSemantic = "BorderColor";>;

// Vertex shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

// Pixel shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture
    float4 texColor = tex2D(TextureSampler, input.TexCoord);
   
    // Check if the pixel is part of the border
    if (texColor.rgb == RarityColor.rgb)
    {
        if (input.TexCoord.x < BorderSize || input.TexCoord.x > 1.0 - BorderSize || 
            input.TexCoord.y < BorderSize || input.TexCoord.y > 1.0 - BorderSize)
        {
            return BorderColor;
        }
    }

    return texColor;
}

// Technique and Pass
technique HighlightTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 MainPS();
    }
}
