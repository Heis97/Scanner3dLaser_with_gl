#version 460 core
layout (triangles, invocations = 4) in;
layout (triangle_strip, max_vertices = 3) out;
