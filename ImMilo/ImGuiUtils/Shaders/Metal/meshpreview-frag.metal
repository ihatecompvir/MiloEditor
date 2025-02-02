#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct FS_out
{
    float4 outputColor [[color(0)]];
};

fragment FS_out FS()
{
    FS_out out = {};
    out.outputColor = float4(1.0, 0.0, 0.0, 1.0);
    return out;
}

