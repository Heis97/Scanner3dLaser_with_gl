#version 460 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (r32f, binding = 0) uniform  image2D inpData;
layout (r32f, binding = 1) uniform  image2D outData;
/*layout (binding = 1) buffer compBuf
{
	float data1[16];
}; */


void main() 
{
	ivec2 pos = ivec2( gl_GlobalInvocationID.xy );

	float in_val = 0;
	for(int i=0; i< 16;i++)
	{
		in_val = in_val+imageLoad(inpData,ivec2(i,0)).r;
	}
	in_val = in_val + 100000 * imageLoad(inpData,pos).r;

	imageStore(outData, pos, vec4(in_val, 0, 0, 0));
}	