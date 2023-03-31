#version 430 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 4) uniform  image2D debug;
layout (rgba32f, binding = 5) uniform  image2D ps1;
layout (rgba32f, binding = 6) uniform  image2D ps2;
layout (rgba32f, binding = 7) uniform  image2D ps_cross;

float len_vec(vec3 v)
{
	return (sqrt(v.x*v.x +v.y * v.y + v.z * v.z));
}
vec3 norm(vec3 v)
{
	float len = len_vec(v);
	if(len==0)
	{
		return(vec3(0,0,0));
	}
	return(vec3(v.x/len,v.y/len,v.z/len));
}



vec3 vec_mul(vec3 v1, vec3 v2)
{
	return(vec3(
	v1.y* v2.z - v1.z* v2.y,
	v1.z* v2.x - v1.x* v2.z,
	v1.x* v2.y - v1.y* v2.x));
}

float scalar_mul(vec3 v1, vec3 v2)
{
	return(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
}

float vec_ang(vec3 v1, vec3 v2)
{
	return(scalar_mul(v1,v2)/(len_vec(v1)*len_vec(v2)));
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
	vec3 v1 = norm(p2-p1);
	vec3 v2 = norm(p3-p1);
	vec3 v3 = norm(vec_mul(v1,v2));
	vec4 flat3d = vec4(v3,-scalar_mul(v3,p1));
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
	float a1 = acos(vec_ang(polygon.v1,v_c));
	float a2 = acos(vec_ang(polygon.v2,v_c));
	float b1 = acos(vec_ang(polygon.v1,polygon.v2));
	if(a1 <= b1 && a2 <=b1)
	{
	   return (vec4(p_cross,1));
	}
	return (vec4(p_cross,0));
}


void main() 
{
	if(gl_GlobalInvocationID.x==0)
	{
	  return;
	}

	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	vec3 p1_0 = imageLoad(ps1,ivec2(0,gl_GlobalInvocationID.y)).xyz;
	vec3 p1_1 = imageLoad(ps1,pos).xyz;
	vec4 p_test = imageLoad(ps1,pos);
	Line line = Line(p1_1-p1_0,p1_0);

	vec4 p2_0 = imageLoad(ps2,ivec2(0,gl_GlobalInvocationID.y));

	for(int i=2; i< imageSize(ps2).x;i++)
	{
		vec4 p2_1 = imageLoad(ps2,ivec2(i-1,gl_GlobalInvocationID.y));
		vec4 p2_2 = imageLoad(ps2,ivec2(i,gl_GlobalInvocationID.y));
		Polygon polygon = create_polygon(p2_0.xyz,p2_1.xyz,p2_2.xyz);
		vec4 crossl =  calc_cross_flat(line, polygon.flat3d);
		vec4 p_cross = cross_line(polygon, line);
		if(p_cross.w>0)
		{
			imageStore(ps_cross, pos, vec4(p_cross));
		}
	}	
}	