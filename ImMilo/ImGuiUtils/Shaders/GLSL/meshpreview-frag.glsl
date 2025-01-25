#version 330
#ifdef GL_ARB_shading_language_420pack
#extension GL_ARB_shading_language_420pack : require
#endif

layout(location = 0) out vec4 outputColor;

void main()
{
    outputColor = vec4(1.0);
}

