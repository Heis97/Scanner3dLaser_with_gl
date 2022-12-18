#version 430 core

layout(location = 0) in vec3 _vertexPosition_model;
layout(location = 1) in vec3 _vertexNormal_model;
layout(location = 2) in vec3 _vertexColor;
layout(location = 3) in vec2 _vertexTexture;

out VS_GS_INTERFACE
{
vec3 vertexPosition_model;
vec3 vertexNormal_model;
vec3 vertexColor;
vec2 vertexTexture;
int InstanceID;
} vs_out;

void main() 
{
    gl_Position = vec4(_vertexPosition_model, 1.0);
	vs_out.vertexPosition_model = _vertexPosition_model;
	vs_out.vertexNormal_model = _vertexNormal_model;
	vs_out.vertexColor = _vertexColor;
	vs_out.vertexTexture = _vertexTexture;
	vs_out.InstanceID  = gl_InstanceID;
}