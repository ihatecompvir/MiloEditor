#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

struct ProjectionMatrixBuffer
{
    float4x4 projection_matrix;
    float4x4 model_matrix;
};

struct VS_out
{
    float4 gl_Position [[position]];
};

struct VS_in
{
    float3 in_position [[attribute(0)]];
};

vertex VS_out VS(VS_in in [[stage_in]], constant ProjectionMatrixBuffer& _16 [[buffer(1)]])
{
    VS_out out = {};
    out.gl_Position = (_16.projection_matrix * _16.model_matrix) * float4(in.in_position, 1.0);
    return out;
}

