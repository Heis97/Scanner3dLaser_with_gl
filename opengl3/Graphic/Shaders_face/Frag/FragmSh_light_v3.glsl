#version 430 core

uniform mat4 LightSource[30];
uniform int light_count;

uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;

//uniform readonly sampler2D texture_im;
layout (rgba32f, binding = 3) uniform  image2D debugdata;
uniform int textureVis;
uniform int lightVis;
uniform float transparency;

uniform int selected;

in GS_FS_INTERFACE
{
	vec3 Position_world;
	vec3 Color;
	vec3 Normal_camera;
	vec3 EyeDirection_camera;
	vec2 TextureUV;
	mat4 Vs[4];
	flat int invoc;
}fs_in;
out vec4 color;

vec3 comp_color_point_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c,vec3 LightVec_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor,vec3 settings,vec3 color_light)
{
	float power_cut = settings.y;
	float cut_off = settings.z;
	vec3 LightColor = color_light;
	float LightPower = power_cut;

	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(LightDirection_c);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);
	

	return(MaterialAmbientColor+
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance)+
		 MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));

}

vec3 comp_color_direct_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c, vec3 LightVec_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor,vec3 settings,vec3 color_light)
{
	float power_cut = settings.y;
	float cut_off = settings.z;
	vec3 LightColor = color_light;
	float LightPower = power_cut;

	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(LightVec_c);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);
	float cos_ang = cut_off ;


	vec3 ld = normalize(LightDirection_c);
	float cosGamma = clamp(dot(l, ld), 0, 1);

	if(cosGamma<cos_ang)
	{
		MaterialDiffuseColor = vec3(0);	
	}

	return(MaterialAmbientColor +
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance) +
		MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));

}

vec3 comp_color_disk_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c, vec3 LightVec_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor,vec3 settings,vec3 color_light)
{
	float power_cut = settings.y;
	float cut_off = settings.z;
	vec3 LightColor = color_light;
	float LightPower = power_cut;

	vec3 z_sh = cross(LightDirection_c,LightVec_c);
	vec3 x_sh = cross(LightVec_c,z_sh);

	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(x_sh);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);

	float cos_ang = cut_off ;


	vec3 ld = normalize(LightDirection_c);
	float cosGamma = clamp(dot(l, ld), 0, 1);


	
	if(cosGamma<cos_ang)
	{
		
		//MaterialDiffuseColor = vec3(0);
		
	}
	LightColor *= 1 -  pow(1-cosGamma,2)*1e9;
	LightColor = clamp(LightColor, 0, 1);
	//if(LightColor
	//imageStore(debugdata, ivec2(0,0), vec4(LightVec_c,111));
	//imageStore(debugdata, ivec2(1,0), vec4(LightVec_c,222));
	return(LightColor);
	/*return(MaterialAmbientColor/distance +
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance) +
		MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));*/

}

vec3 comp_light(mat4 light,vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor)
{
	vec3 direction = light[0].xyz;
	vec3 position = light[1].xyz;
	vec3 color_light = light[2].xyz;
	vec3 settings = light[3].xyz;

	int type = int(settings.x);
	vec3 color_fr = vec3(0);
	vec3 LightDirection_camera = normalize(-(fs_in.Vs[fs_in.invoc] * vec4(direction, 0)).xyz);
	vec3 LightPosition_camera = (fs_in.Vs[fs_in.invoc] * vec4(position, 1)).xyz;
	vec3 LightFall_camera = LightPosition_camera + fs_in.EyeDirection_camera;

	//if(length( LightFall_camera.xy)<1) return(color_light);

	if(type == 1)
	{
		color_fr = comp_color_point_light(position, fs_in.Position_world,
		fs_in.Normal_camera, LightFall_camera, fs_in.EyeDirection_camera,LightDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor, MaterialSpecularColor,settings,color_light);
	}
	else if(type == 2)
	{
		color_fr = comp_color_direct_light(position, fs_in.Position_world,
		fs_in.Normal_camera, LightFall_camera, fs_in.EyeDirection_camera,LightDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor, MaterialSpecularColor,settings,color_light);
	}
	else if(type == 3)
	{
		color_fr = comp_color_disk_light(position, fs_in.Position_world,
		fs_in.Normal_camera, LightFall_camera, fs_in.EyeDirection_camera,LightDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor, MaterialSpecularColor,settings,color_light);
	}

	return(color_fr);

}

void main() {
	vec3 MaterialDiffuseColor = fs_in.Color;
	vec3 MaterialAmbientColor = MaterialAmbient;
	vec3 MaterialSpecularColor = MaterialSpecular;
	
	if (textureVis == 1)
	{
		//MaterialDiffuseColor = texture(textureSample, fs_in.TextureUV).xyz;
		MaterialSpecularColor = 0.2 * MaterialDiffuseColor;
	}
	else
	{
		MaterialDiffuseColor = vec3(0.5);
		MaterialSpecularColor = 0.2 * MaterialDiffuseColor;
	}
	//vec4 color = vec4(0);
	if(lightVis == 1)
	{		
		/*color.xyz = comp_color_point_light(LightPosition_world, fs_in.Position_world,
		fs_in.Normal_camera, fs_in.LightDirection_camera, fs_in.EyeDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor,MaterialSpecularColor);*/
		vec3 MaterialSpecularColor = vec3(0.01);
		for(int i=0; i< light_count; i++)
		{
			color.xyz += comp_light(LightSource[i],
			MaterialAmbientColor, MaterialDiffuseColor,MaterialSpecularColor);
		}
		
	
	}
	else{
		color.xyz = MaterialDiffuseColor;
	}

	color.w = transparency;

	if(selected == 1)
	{
		color.g +=0.2;
		color.w = 0.4;
	}

	//color.w = 0.2;
	//color.xyz += fs_in.Color;
}