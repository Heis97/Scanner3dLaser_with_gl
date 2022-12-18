#version 430 

layout(location = 0) in vec3 _vertexPosition_model;
layout(location = 1) in vec3 _vertexNormal_model;
layout(location = 2) in vec3 _vertexColor;
layout(location = 3) in vec2 _vertexTexture;
uniform mat4 ModelMatrix;

out VS_GS_INTERFACE
{
vec3 vertexPosition_world;
vec3 vertexNormal_world;
vec3 vertexColor;
vec2 vertexTexture;
} vs_out;

void main() 
{	
	vs_out.vertexPosition_world =  (ModelMatrix*vec4(_vertexPosition_model,1)).xyz;
	vs_out.vertexNormal_world =(ModelMatrix * vec4( _vertexNormal_model,0)).xyz;
	vec3 n = normalize(vs_out.vertexNormal_world);
	
	vs_out.vertexColor = _vertexColor;
	vs_out.vertexTexture = _vertexTexture;
	gl_Position = vec4(vs_out.vertexPosition_world,1);

	


	/*vs_out.vertexColor = vec3(0, 0, 0);
	float cosTheta = clamp(dot(n, vec3(1, 0, 0)), 0, 1);

	vs_out.vertexColor +=pow( cosTheta,6) * vec3(0.3, 0, 0);

	float cosTheta2 = clamp(dot(n, vec3(0, 0, 1)), 0, 1);

	vs_out.vertexColor += pow( cosTheta2,6) * vec3(0, 0, 0.3);

	
	if (abs(cosTheta2 - cosTheta) < 0.04)
	{
		vs_out.vertexColor = vec3(0.7, 0.7, 0.7);
	}*/
	/*if (n.x > 0.5)
	{
		vs_out.vertexColor = vec3(0.6, 0, 0);
	}
	if (n.z > 0.5)
	{
		vs_out.vertexColor = vec3(0.0, 0, 0.6);
	}*/
	
	
}