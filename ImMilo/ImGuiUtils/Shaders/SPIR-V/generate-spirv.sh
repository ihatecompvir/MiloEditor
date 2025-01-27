#!/usr/bin/sh
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
  spirv-cross --msl $1 --rename-entry-point "main" $2 $stage --msl-version 302000 --msl-decoration-binding --output "../Metal/$filename.metal"
  
  # I have to do a manual patch to the metal files, so do it before compilation
  if [[ -f "../Metal/$filename.metal.patch" ]]; then
    patch "../Metal/$filename.metal" "../Metal/$filename.metal.patch"
  fi
  
  # If you have the Metal Developer Tools you can set METAL_TOOLS to the bin/ directory and it will compile the metallibs for you
  if [[ -v METAL_TOOLS ]]; then
    METAL_EXEC="$METAL_TOOLS/metal.exe"
    input_dir=$(winepath -w "../Metal/$filename.metal")
    output_dir$(winepath -w "../Metal/$filename.metallib")
  fi
  if [[ "$OSTYPE" == "darwin"* ]]; then
    METAL_EXEC="xcrun -sdk macosx metal"
    input_dir="../Metal/$filename.metal"
    output_dir="../Metal/$filename.metallib"
  fi

  if [[ -v METAL_EXEC ]]; then
    # Did you know you can just slap a Windows executable in Bash and it automatically executes it through Wine?
    $METAL_EXEC $input_dir -o $output_dir
  else
    echo "METAL_EXEC not defined; Metal shaders will not be compiled."
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


