#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct FS_out
{
    float4 outputColor [[color(0)]];
};

struct FS_in
{
    float4 color [[user(locn0)]];
    float2 texCoord [[user(locn1)]];
};

fragment FS_out FS(FS_in in [[stage_in]], texture2d<float> FontTexture [[texture(0)]], sampler FontSampler [[sampler(0)]])
{
    FS_out out = {};
    out.outputColor = in.color * FontTexture.sample(FontSampler, in.texCoord);
    return out;
}

