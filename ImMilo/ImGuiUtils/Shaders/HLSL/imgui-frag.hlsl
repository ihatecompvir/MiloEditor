uniform sampler2D SPIRV_Cross_CombinedFontTextureFontSampler;

static float4 outputColor;
static float4 color;
static float2 texCoord;

struct SPIRV_Cross_Input
{
    float4 color : TEXCOORD0;
    float2 texCoord : TEXCOORD1;
};

struct SPIRV_Cross_Output
{
    float4 outputColor : COLOR0;
};

void frag_main()
{
    outputColor = color * tex2D(SPIRV_Cross_CombinedFontTextureFontSampler, texCoord);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    color = stage_input.color;
    texCoord = stage_input.texCoord;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.outputColor = float4(outputColor);
    return stage_output;
}
