#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct main0_out
{
    float4 outputColor [[color(0)]];
};

struct main0_in
{
    float4 color [[user(locn0)]];
    float2 texCoord [[user(locn1)]];
};

fragment main0_out main0(main0_in in [[stage_in]], texture2d<float> FontTexture [[texture(0)]], sampler FontSampler [[sampler(0)]])
{
    main0_out out = {};
    out.outputColor = in.color * FontTexture.sample(FontSampler, in.texCoord);
    return out;
}

