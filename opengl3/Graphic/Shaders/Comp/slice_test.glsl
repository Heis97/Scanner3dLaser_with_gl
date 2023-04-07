#version 430 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rgba32f, binding = 1) uniform  image2D test_c;

void main() 
{
	imageStore(test_c, ivec2(0,0), vec4(0.1,0.3, 0,0.4));
}