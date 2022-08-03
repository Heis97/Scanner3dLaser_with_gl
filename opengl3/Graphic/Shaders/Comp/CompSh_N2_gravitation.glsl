#version 460 core


layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout (rgba32f, binding = 0) uniform  image2D objdata;
layout (rgba32f, binding = 2) uniform  image2D choosedata;
uniform int targetCamInd;
uniform mat4 VPs[4];
uniform mat4 Vs[4];
uniform vec2 MouseLocGL;
const float deltTime = 1000;
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

void setModelMatr(in float size,in vec3 pos,in vec4 rot,in vec3 _targetCam)
{
	ivec2 ipos4 =  ivec2(4,gl_GlobalInvocationID.y);
	ivec2 ipos5 =  ivec2(5,gl_GlobalInvocationID.y);
	ivec2 ipos6 =  ivec2(6,gl_GlobalInvocationID.y);
	ivec2 ipos7 =  ivec2(7,gl_GlobalInvocationID.y);

	float A = cos(rot.x);
    float B = sin(rot.x);
    float C = cos(rot.y);
    float D = sin(rot.y);
    float E = cos(rot.z);
    float F = sin(rot.z);

	float AD = A * D;
    float BD = B * D;

	imageStore(objdata, ipos4, size*vec4(C * E,-C * F,-D,0));
	imageStore(objdata, ipos5, size*vec4(-BD * E + A * F,BD * F + A * E,-B * C,0));
	imageStore(objdata, ipos6, size*vec4(AD * E + B * F,-AD * F + B * E,A * C,0));
	imageStore(objdata, ipos7, vec4(pos - _targetCam,1));
}

vec4 draw(in float size,in vec3 pos,in vec3 _targetCam)
{
	ivec2 ipos4 =  ivec2(3,gl_GlobalInvocationID.y);

    vec4 pos2d= VPs[0]* vec4(pos - _targetCam,1);
	vec4 pos3d= Vs[0]* vec4(pos - _targetCam,1);
	vec3 pos2dh = pos2d.xyz/pos2d.w;

	float board = 1.2;

	//imageStore(objdata, ipos4,vec4(size,length(pos3d.xyz)/10,length(pos3d.xyz) ,size/length(pos3d.xyz)));
	
	bool vis;
	if(size/length(pos3d.xyz)> 2)
	{
		vis = true;	
	}
	else
	{
		if(size/length(pos3d.xyz)< 1e-3)
		{
			vis = false;	
		}
		else
		{
			if(pos2dh.x<board  && pos2dh.x>-board
			&& pos2dh.y<board && pos2dh.y>-board)
			{
				vis = true;	
			}
			else
			{
				vis = false;	
			}
		}
	}

	
	vec4 cho = vec4(0,0,0,0);
	if(vis)
	{
		 cho.x = 1;
	}
	if(length(MouseLocGL-pos2dh.xy)<(size/length(pos3d.xyz))+0.01)
	{
		cho.y = 1;
	}

	imageStore(objdata, ipos4,vec4(cho.y,length(MouseLocGL-pos2dh.xy),(size/length(pos3d.xyz))+0.01 ,size/abs(pos3d.z)));
	return(cho);
}

void main() 
{
	ivec2 ipos1 = ivec2(0, gl_GlobalInvocationID.y );
	ivec2 ipos2 =  ivec2(1,gl_GlobalInvocationID.y);
	ivec2 ipos3 =  ivec2(2,gl_GlobalInvocationID.y);

	vec3 acs3 = vec3(0,0,0);
	vec4 pos1 = imageLoad(objdata,ipos1);
	vec3 vel1 = imageLoad(objdata,ipos2).rgb;
	float size1 = imageLoad(objdata,ipos2).a;

	vec4 rot1 = imageLoad(objdata,ipos3);

	for(int i=0; i< imageSize(objdata).y; i++)
	{

		if(ipos1.y!=i)      
		{
			ivec2 curP1 = ivec2(0,i);
			vec4 obj = imageLoad(objdata,curP1);
			acs3 += compGravit(pos1.xyz,pos1.a,obj.rgb,obj.a);
		}
	}

	/*if(ipos1.y!=0)      
		{
	ivec2 curP1 = ivec2(0,0);
	vec4 obj = imageLoad(objdata,curP1);
	acs3 += compGravit(pos1.xyz,pos1.a,obj.rgb,obj.a);
	}*/

	pos1.xyz += vel1*deltTime + (acs3*deltTime*deltTime)/2;
	vel1 += acs3*deltTime;
	
	imageStore(objdata, ipos1, pos1);
	imageStore(objdata, ipos2, vec4(vel1, size1));
	vec3 targetC = imageLoad(objdata,ivec2(0, targetCamInd)).xyz;
	setModelMatr(size1,pos1.xyz,rot1,targetC);	
	vec4 choose = draw(rot1.w,pos1.xyz,targetC);
	imageStore(choosedata, ipos1, choose);
	if(choose.y==1)
	{
		imageStore(choosedata, ipos2, vec4(0,targetC));
	}

}	