cbuffer ProjectionMatrixBuffer : register(b0)
{
    row_major float4x4 _16_projection_matrix : packoffset(c0);
    row_major float4x4 _16_model_matrix : packoffset(c4);
};

uniform float4 gl_HalfPixel;

static float4 gl_Position;
static float3 in_position;

struct SPIRV_Cross_Input
{
    float3 in_position : TEXCOORD0;
};

struct SPIRV_Cross_Output
{
    float4 gl_Position : POSITION;
};

void vert_main()
{
    gl_Position = mul(float4(in_position, 1.0f), mul(_16_model_matrix, _16_projection_matrix));
    gl_Position.x = gl_Position.x - gl_HalfPixel.x * gl_Position.w;
    gl_Position.y = gl_Position.y + gl_HalfPixel.y * gl_Position.w;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    in_position = stage_input.in_position;
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    return stage_output;
}
