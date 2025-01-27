#version 330
#ifdef GL_ARB_shading_language_420pack
#extension GL_ARB_shading_language_420pack : require
#endif

layout(binding = 1, std140) uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
    mat4 model_matrix;
} _16;

layout(location = 0) in vec3 in_position;

void main()
{
    gl_Position = (_16.projection_matrix * _16.model_matrix) * vec4(in_position, 1.0);
}

