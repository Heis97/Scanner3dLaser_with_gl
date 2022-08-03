#version 460 core

layout(location = 0) in float ind;

out VS_GS_INTERFACE
{
	float ind;
} vs_out;

void main() 
{
	gl_Position = vec4(0,0,0,1);
	vs_out.ind = ind;

}