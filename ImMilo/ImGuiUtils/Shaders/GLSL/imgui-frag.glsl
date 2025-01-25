#version 330
#ifdef GL_ARB_shading_language_420pack
#extension GL_ARB_shading_language_420pack : require
#endif

uniform sampler2D SPIRV_Cross_CombinedFontTextureFontSampler;

layout(location = 0) out vec4 outputColor;
in vec4 color;
in vec2 texCoord;

void main()
{
    outputColor = color * texture(SPIRV_Cross_CombinedFontTextureFontSampler, texCoord);
}

