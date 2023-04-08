#version 430 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout(rgba32f, binding = 2) uniform  image2D mesh;
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
void save_point(vec3 p, ivec2 size , ivec2 ind_pos,int i)
{
	ivec2 dim = ivec2(imageLoad(isolines, ind_pos).xy); 
	if (dim.x > size.x-1)
	{
		dim.x = 0;
		dim.y++;
	}
	imageStore(isolines, ivec2(3*gl_GlobalInvocationID.x+i,gl_GlobalInvocationID.y), vec4(p, 1));
	dim.x++;
	//imageStore(isolines, ind_pos, vec4(dim.xy, 0, 0));		
}

void find_points_isoline(vec4 surf,vec3 vertexPosition_world[3])
{
	ivec2 size = imageSize(isolines);
	ivec2 ind_pos = ivec2(size.x - 1, size.y - 1);
	for (int i = 0; i < 3; i++)
	{
		vec3 p_aff = vec3(0);
		int i1 = i + 1; if (i1 > 2) i1 = 0;
		if (cross_affil(vertexPosition_world[i], vertexPosition_world[i1], surf, p_aff))
		{
			save_point(p_aff,size,ind_pos,i);
		}
	}
}

void main() 
{
	vec3 vertexPosition_world[3];
	for (int i=0; i<3;i++)
	{
		vertexPosition_world[i] = imageLoad(mesh,ivec2(3*gl_GlobalInvocationID.x+i,gl_GlobalInvocationID.y)).xyz;	
	}
	find_points_isoline(surf_cross,vertexPosition_world);	

}