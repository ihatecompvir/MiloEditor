static float4 outputColor;

struct SPIRV_Cross_Output
{
    float4 outputColor : COLOR0;
};

void frag_main()
{
    outputColor = 1.0f.xxxx;
}

SPIRV_Cross_Output main()
{
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.outputColor = float4(outputColor);
    return stage_output;
}
