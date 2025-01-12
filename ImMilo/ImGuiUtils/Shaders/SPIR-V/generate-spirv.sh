#!/usr/bin/bash
glslang -V imgui-vertex.glsl -o imgui-vertex.spv -S vert
glslang -V imgui-frag.glsl -o imgui-frag.spv -S frag
glslang -V meshpreview-vertex.glsl -o meshpreview-vertex.spv -S vert
glslang -V meshpreview-frag.glsl -o meshpreview-frag.spv -S frag
