#version 460 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 5) uniform  image2D ps1;
layout (rgba32f, binding = 6) uniform  image2D ps2;
layout (rgba32f, binding = 7) uniform  image2D ps_cross;

float scalar_mul(vec3 v1, vec3 v2)
{
	return(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
}

struct Line
{
	vec3 k;
	vec3 p;
};

struct Polygon
{
	vec3 p1;
	vec3 v1;
	vec3 v2;
	vec3 v3;
	vec4 flat3d;
};

Polygon create_polygon(vec3 p1,vec3 p2,vec3 p3)
{
	vec3 v1 = normalize(p2-p1);
	vec3 v2 = normalize(p3-p1);
	vec3 v3 = normalize(cross(v1,v2));
	vec4 flat3d = vec4(v3,scalar_mul(v3,p1));
	return (Polygon(p1,v1,v2,v3,flat3d));
}

vec4 calc_cross_flat(Line line, vec4 flat1)
{
	float del = scalar_mul(line.k,flat1.xyz);
	float num = scalar_mul(line.p,flat1.xyz)+flat1.w;
	if(del!=0)
	{
		float t = -num/del;
		vec3 p = t*line.k+line.p;
		return (vec4(p,1));
	}
	return (vec4(0,0,0,0));
}

vec4 cross_line(Polygon polygon, Line line)
{
	vec3 p_cross = calc_cross_flat(line,polygon.flat3d).xyz;
	vec3 v_c = p_cross - polygon.p1;
	float a1 = dot(polygon.v1,v_c);
	float a2 = dot(polygon.v2,v_c);
	float b1 = dot(polygon.v1,polygon.v2);
	if(a1 <= b1 && a2 <=b1)
	{
	 return (vec4(p_cross,1));
	}
	return (vec4(0,0,0,0));
}


void main() 
{
	if(gl_GlobalInvocationID.x==0)
	{
	  return;
	}

	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	vec3 p1_0 = imageLoad(ps1,ivec2(0,0)).xyz;
	vec3 p1_1 = imageLoad(ps1,pos).xyz;
	vec4 p_test = imageLoad(ps1,pos);
	Line line = Line(p1_1-p1_0,p1_0);

	vec4 p2_0 = imageLoad(ps2,ivec2(0,0));
	for(int i=2; i< imageSize(ps2).x;i++)
	{
		vec4 p2_1 = imageLoad(ps2,ivec2(i-1,0));
		vec4 p2_2 = imageLoad(ps2,ivec2(i,0));
		Polygon polygon = create_polygon(p2_0.xyz,p2_1.xyz,p2_2.xyz);
		vec4 p_cross = cross_line(polygon, line);
		if(p_cross.w>0)
		{
			imageStore(ps_cross, pos, p_cross);
		}
	}	

	//imageStore(ps_cross, pos, vec4(p_test.xyz,gl_GlobalInvocationID.x));
}	