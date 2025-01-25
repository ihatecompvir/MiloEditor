uniform float4 ProjectionMatrixBuffer[4];

static float4 gl_Position;
static float2 in_position;
static float4 color;
static float4 in_color;
static float2 texCoord;
static float2 in_texCoord;

struct SPIRV_Cross_Input
{
    float2 in_position : TEXCOORD0;
    float2 in_texCoord : TEXCOORD1;
    float4 in_color : TEXCOORD2;
};

struct SPIRV_Cross_Output
{
    float4 color : TEXCOORD0;
    float2 texCoord : TEXCOORD1;
    float4 gl_Position : SV_Position;
};

void vert_main()
{
    gl_Position = mul(float4(in_position, 0.0f, 1.0f), float4x4(ProjectionMatrixBuffer[0], ProjectionMatrixBuffer[1], ProjectionMatrixBuffer[2], ProjectionMatrixBuffer[3]));
    color = in_color;
    texCoord = in_texCoord;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    in_position = stage_input.in_position;
    in_color = stage_input.in_color;
    in_texCoord = stage_input.in_texCoord;
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output.color = color;
    stage_output.texCoord = texCoord;
    return stage_output;
}
