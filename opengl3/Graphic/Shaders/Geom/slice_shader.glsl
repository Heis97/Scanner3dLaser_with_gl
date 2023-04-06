#version 430 core
layout (triangles, invocations = 1) in;

layout(rgba32f, binding = 3) uniform  image2D isolines;

uniform vec3 LightPosition_world;
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
vec2 TextureUV;
} fs_in;

//-------------------------------------------------------------------------------------
bool affil_segment(vec3 p_st, vec3 p_end, vec3 p_ch)
{
	float dist_0 = length((p_end - p_st));
	float dist_1 = length((p_ch - p_st));
	float dist_2 = length((p_ch - p_end));
	if ((dist_1 < dist_0) && (dist_2 < dist_0))
	{
		return (true);
	}	
	return (false);
}
float scalar_mul(vec3 v1, vec3 v2)
{
	return(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
}

bool cross_affil(vec3 p1, vec3 p2,vec4 flat1, out vec3 p_aff)
{
	vec3 v = p2 - p1;
	vec3 p = p1;

	if (scalar_mul(v,flat1.xyz) == 0)
	{
		return (false);
	}	
	float t = (-flat1.w - scalar_mul(p,flat1.xyz)) / (scalar_mul(v,flat1.xyz));
	vec3 p_c = p + v * t;
	if (affil_segment(p1, p2, p_c))
	{
		p_aff = p_c;
		return (true);
	}	
	else
	{
		return (false);
	}
}
void save_point(vec3 p, int y)
{
	ivec2 ind_pos = ivec2(imageSize(isolines).x - 1, y);
	int i = int(imageLoad(isolines, ind_pos).x); ;
	if (i < 7990)
	{
		imageStore(isolines, ivec2(i, y), vec4(p, 0));
		i++;
		imageStore(isolines, ind_pos, vec4(i, 0, 0, 0));
	}
}
void find_points_isoline(vec4 surf,int y)
{
	for (int i = 0; i < 3; i++)
	{
		vec3 p_aff = vec3(0);
		int i1 = i + 1; if (i1 > 2) i1 = 0;
		if (cross_affil(vs_out[i1].vertexPosition_world, vs_out[i].vertexPosition_world, surf, p_aff))
		{
			save_point(p_aff,y);
			return;
		}
	}
}

void main() 
{
	
	gl_ViewportIndex = gl_InvocationID;
	vec4 A = Vs[gl_InvocationID] * vec4(vs_out[0].vertexPosition_world, 1.0);
	vec4 B = Vs[gl_InvocationID] * vec4(vs_out[1].vertexPosition_world, 1.0);
	vec4 C = Vs[gl_InvocationID] * vec4(vs_out[2].vertexPosition_world, 1.0);
	vec4 n = vec4(normalize((comp_norm(A.xyz, B.xyz, C.xyz))), 0);
	bool selected_triangle = checkPointInTriangle(A.xy,B.xy,C.xy,MouseLocGL);
	
	if (comp_proj == 1)
	{
		//compProjection(Vs[gl_InvocationID],A,B,C,n);
		for (int i = 0; i < surfs_len; i++)
		{
			find_points_isoline(surfs_cross[i],i);
		}
		
	}
}