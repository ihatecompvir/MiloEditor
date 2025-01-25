Texture2D<float4> FontTexture;
SamplerState FontSampler;

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
    float4 outputColor : SV_Target0;
};

void frag_main()
{
    outputColor = color * FontTexture.Sample(FontSampler, texCoord);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    color = stage_input.color;
    texCoord = stage_input.texCoord;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.outputColor = outputColor;
    return stage_output;
}
