#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct ProjectionMatrixBuffer
{
    float4x4 projection_matrix;
    float4x4 model_matrix;
};

struct main0_out
{
    float4 gl_Position [[position]];
};

struct main0_in
{
    float3 in_position [[attribute(0)]];
};

vertex main0_out main0(main0_in in [[stage_in]], constant ProjectionMatrixBuffer& _16 [[buffer(0)]])
{
    main0_out out = {};
    out.gl_Position = (_16.projection_matrix * _16.model_matrix) * float4(in.in_position, 1.0);
    return out;
}

