#!/usr/bin/bash
glslang -V imgui-vertex.glsl -o imgui-vertex.spv -S vert
glslang -V imgui-frag.glsl -o imgui-frag.spv -S frag
glslang -V meshpreview-vertex.glsl -o meshpreview-vertex.spv -S vert
glslang -V meshpreview-frag.glsl -o meshpreview-frag.spv -S frag

convert_glsl() {
  local filename=$(basename $1 .spv)
  spirv-cross --version 330 --no-es $1 --output "../GLSL/$filename.glsl"
}

convert_hlsl() {
  local filename=$(basename $1 .spv)
  spirv-cross --hlsl --shader-model 50 --hlsl-enable-compat --hlsl-flatten-matrix-vertex-input-semantics $1 --output "../HLSL/$filename.hlsl"
  # If you have dxc you can set DXC_PATH to its path and this script will compile the HLSL for you
  if [[ -v DXC_PATH ]]; then
    if [[ $filename == *"frag"* ]]; then
      model="ps_5_0"
    else
      model="vs_5_0"
    fi
    $DXC_PATH -T $model "../HLSL/$filename.hlsl" -Fo "../HLSL/$filename.hlsl.bytes"
  fi
}

convert_metal() {
  local filename=$(basename $1 .spv)
  spirv-cross --msl $1 --output "../Metal/$filename.metal"
}

convert_all() {
  convert_glsl $1
  convert_hlsl $1
  convert_metal $1
}

convert_all imgui-frag.spv
convert_all imgui-vertex.spv
convert_all meshpreview-frag.spv
convert_all meshpreview-vertex.spv


