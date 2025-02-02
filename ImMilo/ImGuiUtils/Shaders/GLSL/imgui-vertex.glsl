#version 330
#ifdef GL_ARB_shading_language_420pack
#extension GL_ARB_shading_language_420pack : require
#endif

layout(binding = 0, std140) uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
} _16;

layout(location = 0) in vec2 in_position;
out vec4 color;
layout(location = 2) in vec4 in_color;
out vec2 texCoord;
layout(location = 1) in vec2 in_texCoord;

void main()
{
    gl_Position = _16.projection_matrix * vec4(in_position, 0.0, 1.0);
    color = in_color;
    texCoord = in_texCoord;
}

