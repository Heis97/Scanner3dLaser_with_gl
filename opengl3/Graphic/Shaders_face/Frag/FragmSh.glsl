#version 430 core
uniform vec3 LightPosition_world;
uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;
uniform float lightPower;
uniform sampler2D textureSample;
uniform int textureVis;
uniform int lightVis;
uniform float transparency;

in GS_FS_INTERFACE
{
	vec3 Position_world;
	vec3 Color;
	vec3 Normal_camera;
	vec3 EyeDirection_camera;
	vec3 LightDirection_camera;
	vec2 TextureUV;
}fs_in;
out vec4 color;

vec3 comp_color(vec3 LightPosition_w, vec3 Position_w, vec3 Normal_c,
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
		color.xyz = comp_color(LightPosition_world, fs_in.Position_world,
		fs_in.Normal_camera, fs_in.LightDirection_camera, fs_in.EyeDirection_camera,
		MaterialAmbientColor, MaterialDiffuseColor,MaterialSpecularColor);
	
	}
	else{
		color.xyz = MaterialDiffuseColor;
	}

	color.w = transparency;
	color.w = 0.2;
	//color.xyz += fs_in.Color;
}