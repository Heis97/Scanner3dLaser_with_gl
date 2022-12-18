#version 430
in vec3 pos;
out vec2 texCoord;
void main() {
	texCoord = pos.xy*0.5f + 0.5f;
	gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
}