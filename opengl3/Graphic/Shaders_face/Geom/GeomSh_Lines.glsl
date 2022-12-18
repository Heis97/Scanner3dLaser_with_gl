#version 430 
layout (lines, invocations = 4) in;
layout (line_strip, max_vertices = 6) out;

uniform vec3 LightPosition_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;

in VS_GS_INTERFACE
{
vec3 vertexPosition_world;
vec3 vertexNormal_world;
vec3 vertexColor;
vec2 vertexTexture;
}vs_out[];

out GS_FS_INTERFACE
{
vec3 Position_world;
vec3 Color;
vec3 Normal_camera;
vec3 EyeDirection_camera;
vec3 LightDirection_camera;
vec2 TextureUV;
} fs_in;

void main() 
{
	gl_ViewportIndex = gl_InvocationID;
   for (int i = 0; i < gl_in.length(); i++)
   { 
	    
		gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0);

	    fs_in.Position_world = gl_Position.xyz;
	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;
	    EmitVertex();
	}

}
