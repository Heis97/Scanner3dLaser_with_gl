#version 430 core
layout (triangles, invocations = 1) in;
layout(triangle_strip, max_vertices = 3) out;

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
uniform vec4 surf_cross;



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
void save_point(vec3 p, ivec2 size , ivec2 ind_pos)
{
	ivec2 dim = ivec2(imageLoad(isolines, ind_pos).xy); 
	if (dim.x > size.x-1)
	{
		dim.x = 0;
		dim.y ++;
	}
	imageStore(isolines, dim, vec4(p, 1));
	dim.x++;
	imageStore(isolines, ivec2(1, 1), vec4(3,0, 0, 1));//debug
	imageStore(isolines, ind_pos, vec4(dim.xy, 0, 1));		
}
void find_points_isoline(vec4 surf)
{
	imageStore(isolines, ivec2(3, 3), vec4(1, 0, 0, 1));//debug
	ivec2 size = imageSize(isolines);
	ivec2 ind_pos = ivec2(size.x - 1, size.y - 1);
	imageStore(isolines, ivec2(4, 4), vec4(0, 0, 4, 1));//debug
	for (int i = 0; i < 3; i++)
	{
		vec3 p_aff = vec3(0);
		int i1 = i + 1; if (i1 > 2) i1 = 0;
		if (cross_affil(vs_out[i1].vertexPosition_world, vs_out[i].vertexPosition_world, surf, p_aff))
		{
			save_point(p_aff,size,ind_pos);
			return;
		}
		
	}
	imageStore(isolines, ivec2(0, 0), vec4(0, 2, 0, 1));//debug
	imageStore(isolines, ivec2(1, 0), vec4(0, 1, 0, 1));//debug
	imageStore(isolines, ivec2(0, 1), vec4(0, 3, 0, 1));//debug
}



void main() 
{
	//imageStore(isolines, ivec2(0, 0), vec4(-2, 0, 0, 0));
	find_points_isoline(surf_cross);
	//imageStore(isolines, ivec2(0, 0), vec4(1, 20, 40, 0));
	//for (int i=0; i<100;i++)
		//imageStore(isolines, ivec2(i,0), vec4(1*i,2, 4, 8));
}