#version 460 core

layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 0) uniform  image2D posData;
layout (rgba32f, binding = 1) uniform  image2D posDataCompG;
layout (rgba32f, binding = 2) uniform  image2D velData;
layout (r32f, binding = 3) uniform  image2D massData;
layout (rgba32f, binding = 4) uniform  image2D acsData;
/*layout (binding = 1) buffer compBuf
{
	float data1[16];
}; */
const float deltTime = 1;
const float G = 1.18656E-19;
//1 - объект расчёта, 2 - влияющие на него другие объекты
//масса в массах земли, расстояние в астрономических единицах
vec3 compGravit(in vec3 pos1, in float mass1,in vec3 pos2,in float mass2)
{
	float dist = distance(pos1,pos2);
	if(dist<1.0E-9)
	{
		dist = 1.0E-9;
	}
	float a = (G*mass2)/(dist*dist);
	vec3 a3 = ((pos2 - pos1)/dist)*a;
	return(a3);
}

void main() 
{
	ivec2 ipos = ivec2( gl_GlobalInvocationID.xy );
	vec3 acs3 = vec3(0,0,0);
	vec3 pos1 = imageLoad(posData,ipos).rgb;
	vec3 vel1 = imageLoad(velData,ipos).rgb;
	float mass1 = imageLoad(massData,ipos).r;
	for(int i=0; i< imageSize(massData).x; i++)
	{

		if(ipos.x!=i)
		{
			ivec2 curP = ivec2(i,0);
			acs3 += compGravit(pos1,mass1,imageLoad(posData,curP).rgb,imageLoad(massData,curP).r);
		}
	}
	
	pos1 += vel1*deltTime + (acs3*deltTime*deltTime)/2;
	vel1 += acs3*deltTime;
	
	imageStore(acsData, ipos, vec4(acs3, 0));
	imageStore(posData, ipos, vec4(pos1, 0));
	imageStore(velData, ipos, vec4(vel1, 0));
}	