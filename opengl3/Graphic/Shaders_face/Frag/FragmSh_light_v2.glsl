#version 430 core
uniform vec3 LightPosition_world;


uniform mat4 LightSource[30];

uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;

uniform float lightPower;
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
	vec3 LightDirection_camera;
	vec3 LightVec_camera;
	vec2 TextureUV;
}fs_in;
out vec4 color;

vec3 comp_color_point_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor)
{
	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(LightDirection_c);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);
	vec3 LightColor = vec3(1.0, 1.0, 1.0);
	float LightPower = lightPower;
	
	return(MaterialAmbientColor+
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance)+
		 MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));

}

vec3 comp_color_direct_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c, vec3 LightVec_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor)
{
	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(LightVec_c);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);
	vec3 LightColor = vec3(1.0, .0, 1.0);
	float LightPower = lightPower;
	float cos_ang = 0.9999;


	vec3 ld = normalize(LightDirection_c);
	float cosGamma = clamp(dot(l, ld), 0, 1);

	if(cosGamma<cos_ang)
	{
		MaterialDiffuseColor = vec3(0);	
	}
	MaterialSpecularColor = vec3(0.001);
	return(MaterialAmbientColor +
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance) +
		MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));

}

vec3 comp_color_disk_light(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
	vec3 LightDirection_c, vec3 EyeDirection_c, vec3 LightVec_c,
	vec3 MaterialAmbientColor, vec3 MaterialDiffuseColor, vec3 MaterialSpecularColor)
{

	vec3 z_sh = cross(LightDirection_c,LightVec_c);
	vec3 x_sh = cross(LightVec_c,z_sh);

	float distance = length(LightPosition_w - Position_w);
	vec3 n = normalize(Normal_c);
	vec3 l = normalize(x_sh);
	float cosTheta = clamp(dot(n, l), 0, 1);
	vec3 E = normalize(EyeDirection_c);
	vec3 R = reflect(-l, n);
	float cosAlpha = clamp(dot(E, R), 0, 1);
	vec3 LightColor = vec3(1.0, .0, 0.0);
	float LightPower = lightPower;
	float cos_ang = 0.9999;


	vec3 ld = normalize(LightDirection_c);
	float cosGamma = clamp(dot(l, ld), 0, 1);


	//imageStore(debugdata, ivec2(0,0), vec4(LightVec_c,111));
	//imageStore(debugdata, ivec2(1,0), vec4(LightVec_c,222));
	if(cosGamma<cos_ang)
	{
		MaterialDiffuseColor *= (cosGamma-0.999) * 50;
	}
	MaterialSpecularColor = vec3(0.01);
	return(MaterialAmbientColor/distance +
		MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance * distance) +
		MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha, 5) / (distance * distance));

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

	if(lightVis == 1)
	{		
		/*color.xyz = comp_color_point_light(LightPosition_world, fs_in.Position_world,
		fs_in.Normal_camera, fs_in.LightDirection_camera, fs_in.EyeDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor,MaterialSpecularColor);*/

		color.xyz = comp_color_disk_light(LightPosition_world, fs_in.Position_world,
		fs_in.Normal_camera, fs_in.LightDirection_camera, fs_in.EyeDirection_camera,fs_in.LightVec_camera,
		MaterialAmbientColor, MaterialDiffuseColor,MaterialSpecularColor);
	
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