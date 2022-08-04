#version 460 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 4) uniform  image2D debug;
layout (rgba32f, binding = 5) uniform  image2D ps1;
layout (rgba32f, binding = 6) uniform  image2D ps2;
layout (rgba32f, binding = 7) uniform  image2D ps_cross;

double len_vec(dvec3 v)
{
	return (sqrt(v.x*v.x +v.y * v.y + v.z * v.z));
}

dvec3 norm(dvec3 v)
{
	double len = len_vec(v);
	if(len==0)
	{
		return(dvec3(0,0,0));
	}
	return(dvec3(v.x/len,v.y/len,v.z/len));
}



dvec3 vec_mul(dvec3 v1, dvec3 v2)
{
	return(dvec3(
	v1.y* v2.z - v1.z* v2.y,
	v1.z* v2.x - v1.x* v2.z,
	v1.x* v2.y - v1.y* v2.x));
}

double scalar_mul(dvec3 v1, dvec3 v2)
{
	return(v1.x * v2.x + v1.y * v2.y + v1.z * v2.z);
}

double vec_ang(dvec3 v1, dvec3 v2)
{
	return(scalar_mul(v1,v2)/(len_vec(v1)*len_vec(v2)));
}
struct Line
{
	dvec3 k;
	dvec3 p;
};

struct Polygon
{
	dvec3 p1;
	dvec3 v1;
	dvec3 v2;
	dvec3 v3;
	dvec4 flat3d;
};

Polygon create_polygon(dvec3 p1,dvec3 p2,dvec3 p3)
{
	dvec3 v1 = norm(p2-p1);
	dvec3 v2 = norm(p3-p1);
	dvec3 v3 = norm(vec_mul(v1,v2));
	dvec4 flat3d = dvec4(v3,-scalar_mul(v3,p1));
	return (Polygon(p1,v1,v2,v3,flat3d));
}

dvec4 calc_cross_flat(Line line, dvec4 flat1)
{
	double del = scalar_mul(line.k,flat1.xyz);
	
	double num = scalar_mul(line.p,flat1.xyz)+flat1.w;
	if(del!=0)
	{
		double t = -num/del;
		dvec3 p = t*line.k+line.p;
		return (dvec4(p,1));
	}
	return (dvec4(0,0,0,0));
}

dvec4 cross_line(Polygon polygon, Line line)
{
	dvec3 p_cross = calc_cross_flat(line,polygon.flat3d).xyz;
	dvec3 v_c = p_cross - polygon.p1;
	double a1 = acos(float(vec_ang(polygon.v1,v_c)));
	double a2 = acos(float(vec_ang(polygon.v2,v_c)));
	double b1 = acos(float(vec_ang(polygon.v1,polygon.v2)));
	if(a1 <= b1 && a2 <=b1)
	{
	   return (dvec4(p_cross,1));
	}
	return (dvec4(p_cross,0));
}


void main() 
{
	if(gl_GlobalInvocationID.x==0)
	{
	  return;
	}

	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	dvec3 p1_0 = imageLoad(ps1,ivec2(0,gl_GlobalInvocationID.y)).xyz;
	dvec3 p1_1 = imageLoad(ps1,pos).xyz;
	dvec4 p_test = imageLoad(ps1,pos);
	Line line = Line(p1_1-p1_0,p1_0);

	dvec4 p2_0 = imageLoad(ps2,ivec2(0,gl_GlobalInvocationID.y));

	for(int i=2; i< imageSize(ps2).x;i++)
	{
		dvec4 p2_1 = imageLoad(ps2,ivec2(i-1,gl_GlobalInvocationID.y));
		dvec4 p2_2 = imageLoad(ps2,ivec2(i,gl_GlobalInvocationID.y));
		Polygon polygon = create_polygon(p2_0.xyz,p2_1.xyz,p2_2.xyz);
		dvec4 crossl =  calc_cross_flat(line, polygon.flat3d);
		dvec4 p_cross = cross_line(polygon, line);
		if(p_cross.w>0)
		{
			imageStore(ps_cross, pos, vec4(p_cross));
		}

		
	}	
	
	
	//imageStore(ps_cross, pos, vec4(gl_GlobalInvocationID.xy,0,0));
	//imageStore(ps_cross, pos, vec4(p_test.xyz,gl_GlobalInvocationID.x));
}	