#version 460 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 0) uniform  image2D aux_data;




vec4 comp_pores_45(vec4 inp)
{
	float h = inp.r;
	float l = inp.g;
	float t = inp.b;
	float theta = inp.a;

	float triangle_area = ((t * sin(radians(45))) * (t * cos(radians(45))));
    float triangle_volume = triangle_area * t / 2;
    float rectangle_area = h * 2 * t * cos(radians(45));
    float rectangle_volume = rectangle_area * t / 2;
    float h_volume = rectangle_volume + triangle_volume * 2;

    // Объем бокового ребра
    float square_area = t * t;
    float l_volume = square_area * l * sin(radians(theta));

    // Объем элементарной ячейки
    float unit_cell_volume = 32 * (h_volume + l_volume);

    // Общий объем / размер пор
    float height_cell = 2 * (l * cos(radians(90 - theta))) + 2 * t;
    float pore_size_B_B = 2 * (h * sin(radians(45)));
    float pore_size_A_A = pore_size_B_B - (2 * l * sin(radians(theta))) * (1 / tan(radians(theta)));
    float width_cell = pore_size_B_B + 2 * t + pore_size_A_A;
    float general_volume = height_cell * width_cell * width_cell;

    // Пористость
    float porosity = (1 - unit_cell_volume / general_volume) * 100;
   
    return (vec4 ( porosity, pore_size_A_A * 0.1, pore_size_B_B * 0.1,0));

}

void main() 
{

	ivec2 ipos0 = ivec2(0,gl_GlobalInvocationID.x);
	ivec2 ipos1 = ivec2(1,gl_GlobalInvocationID.x);

	vec4 inp = imageLoad(aux_data,ipos0);
	vec4 outp = vec4(comp_pores_45(inp));

	imageStore(aux_data, ipos1, outp );
	
}	

