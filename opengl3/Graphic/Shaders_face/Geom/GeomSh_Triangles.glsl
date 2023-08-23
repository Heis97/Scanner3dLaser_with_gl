#version 430 core
layout (triangles, invocations = 4) in;
layout (triangle_strip, max_vertices = 3) out;
layout (rgba32f, binding = 3) uniform  image2D debugdata;
uniform vec3 LightPosition_world;
uniform vec3 LightVec_world;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform mat4 Ps[4];
uniform vec4 surfs_cross[30];
uniform vec2 MouseLoc;
uniform vec2 MouseLocGL;
uniform int comp_proj;
uniform int render_count;
uniform int show_faces;
uniform int inv_norm;
uniform int surfs_len;


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
	vec3 LightVec_camera;
	vec2 TextureUV;
	mat4 Vs[4];
	int gl_ind;
} fs_in;


void main() 
{
	
   gl_ViewportIndex = gl_InvocationID;

   for (int i = 0; i < gl_in.length(); i++)
   { 	    
        gl_Position = VPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0);

	    fs_in.Position_world = vs_out[i].vertexPosition_world;
	    vec3 vertexPosition_camera = (Vs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_world, 1.0)).xyz;
	    fs_in.EyeDirection_camera = vec3(0,0,0) - vertexPosition_camera;
	    vec3 LightPosition_camera = ( Vs[gl_InvocationID] * vec4(LightPosition_world,1)).xyz;
		fs_in.LightVec_camera = normalize(-(Vs[gl_InvocationID] * vec4(LightVec_world, 0)).xyz);
	    fs_in.LightDirection_camera = LightPosition_camera + fs_in.EyeDirection_camera;
	    fs_in.Normal_camera = ( Vs[gl_InvocationID] * vec4(vs_out[i].vertexNormal_world, 0)).xyz;
		if (show_faces == 1)
		{
			if (inv_norm == 1)
			{
				fs_in.Normal_camera *= -1;
			}
		}
	    fs_in.Color = vs_out[i].vertexColor;
		fs_in.TextureUV = vs_out[i].vertexTexture;
		fs_in.Vs = Vs;
		fs_in.gl_ind = gl_InvocationID;
	    EmitVertex();
	}
}
