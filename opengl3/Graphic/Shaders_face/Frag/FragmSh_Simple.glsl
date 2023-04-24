#version 430 core
uniform vec3 LightPosition_world;
uniform vec3 MaterialDiffuse;
uniform vec3 MaterialAmbient;
uniform vec3 MaterialSpecular;
uniform float lightPower;
uniform sampler2D textureSample;
uniform int textureVis;
uniform float transparency;
uniform int selected;

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
void main() {

	/*if (textureVis == 1)
	{
		color = texture( textureSample,  fs_in.TextureUV );
	}
	else
	{
	    
	}*/
	color.xyz = fs_in.Color;

	color.w = transparency;

	if(selected == 1)
	{
		color.g +=0.2;
		color.w = 0.4;
	}
}