#version 430 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 4) uniform  image2D ps_intersec;
layout (rgba32f, binding = 5) uniform  image2D pols1;
layout (rgba32f, binding = 6) uniform  image2D pols2;

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

vec4 cross_line_triang(Polygon polygon, Line line)
{
	vec3 p_cross = calc_cross_flat(line,polygon.flat3d).xyz;
	vec3 v_c = p_cross - polygon.v3;
	float a1 = vec_ang(polygon.v1,v_c);
	float a2 = vec_ang(polygon.v2,v_c);
	float b1 = vec_ang(polygon.v1,polygon.v2);
	if(a1 > b1 || a2 >b1)
	{
	   return (vec4(p_cross,0));
	}
	v_c = p_cross - polygon.v2;
	a1 = vec_ang(polygon.v1,v_c);
	a2 = vec_ang(polygon.v3,v_c);
	b1 = vec_ang(polygon.v1,polygon.v3);
	if(a1 > b1 || a2 > b1)
	{
	   return (vec4(p_cross,0));
	}
	return (vec4(p_cross,1));
}

vec4[2] cross_line_triang(Polygon pn1, Polygon pn2)
{
	int ind = 0;
	vec4[] ps1 = vec4[2](vec4(0),vec4(0));
	vec4[] ps2 = vec4[6](vec4(0),vec4(0),vec4(0),vec4(0),vec4(0),vec4(0));

	ps2[0]  = cross_line_triang(pn1,Line(pn2.v1,pn2.v2));
	ps2[1]  = cross_line_triang(pn1,Line(pn2.v2,pn2.v3));
	ps2[2]  = cross_line_triang(pn1,Line(pn2.v3,pn2.v1));

	ps2[3]  = cross_line_triang(pn2,Line(pn1.v1,pn1.v2));
	ps2[4]  = cross_line_triang(pn2,Line(pn1.v2,pn1.v3));
	ps2[5]  = cross_line_triang(pn2,Line(pn1.v3,pn1.v1));

	for(int i=0; i<6; i++)
	{
		if(ps2[i].w == 1)
		{
			ps1[ind] = ps2[i];
			ind++;
		}
	}

	return (vec4[2](ps1[0],ps1[1]));
}


void main() 
{

	vec3 vp1[3];
	for (int i=0; i<3;i++)
	{
		vp1[i] = imageLoad(pols1,ivec2(3*gl_GlobalInvocationID.x+i,gl_GlobalInvocationID.y)).xyz;	
	}
	Polygon polygon1 = create_polygon(vp1[0],vp1[1],vp1[2]);
	vec3 vp2[3];
	for (int i=0; i<3;i++)
	{
		vp2[i] = imageLoad(pols2,ivec2(3*gl_GlobalInvocationID.x+i,gl_GlobalInvocationID.y)).xyz;	
	}
	Polygon polygon2 = create_polygon(vp2[0],vp2[1],vp2[2]);
	vec4[2] inter = cross_line_triang(polygon1,polygon2);

	imageStore(ps_cross, pos, vec4(p_cross));

}	