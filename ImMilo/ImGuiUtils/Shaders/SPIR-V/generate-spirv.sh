#!/usr/bin/bash
# This script only works on Linux.
# TODO: Make a version of this script for Windows.
# This script might work under WSL, however.

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
  if [[ $2 == "FS" ]]; then
    stage="frag"
  fi
  if [[ $2 == "VS" ]]; then
    stage="vert"
  fi
  echo "Compiling $filename, entry point: $2, stage: $stage"
  spirv-cross --msl $1 --rename-entry-point "main" $2 $stage --output "../Metal/$filename.metal"
  # If you have the Metal Developer Tools you can set METAL_TOOLS to the bin/ directory and it will compile the metallibs for you
  if [[ -v METAL_TOOLS ]]; then
    # Did you know you can just slap a Windows executable in Bash and it automatically executes it through Wine?
    "$METAL_TOOLS/metal.exe" $(winepath -w "../Metal/$filename.metal") -o $(winepath -w "../Metal/$filename.metallib")
  else
    echo "METAL_TOOLS not defined; Metal shaders will not be compiled."
  fi
}

convert_all() {
  convert_glsl $1
  # DirectX is not supported currently
  #convert_hlsl $1
  convert_metal $1 $2
}

convert_all imgui-frag.spv FS
convert_all imgui-vertex.spv VS
convert_all meshpreview-frag.spv FS
convert_all meshpreview-vertex.spv VS


