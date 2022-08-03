#version 460 core
layout (local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
layout (rgba32f, binding = 0) uniform  image2D aux_data;
layout (rgba32f, binding = 1) uniform  image2D porosity_map;

uniform vec2 limits_h;
uniform vec2 limits_l;
uniform vec2 limits_t;
uniform vec2 limits_theta;
uniform float cur_l;

uniform float porosity_inp;
uniform float pore_size_inp;

uniform int type_comp;

uniform ivec4 sizeXY;//map_comp xy, map_poros z, map_por_depth w 

vec4 comp_pores_90(float h,float l ,float t, float theta)
{
	float unit_cell_volume = 32 * t*t * (h - t + 2 * l * sin(radians(theta)));

    //Общий объем / размер пор
    float height_cell = 4 * t + 2 * l * sin(radians(theta));
    float width_cell =2 * (h - 2 * t - l * cos(radians(theta)));
    float general_volume = height_cell * width_cell * width_cell;
    float pore_size_B_B = -2 * t + width_cell / 2;
    float pore_size_A_A = h - 2 * l * cos(radians(theta)) - 4 * t;

    //Пористость
    float porosity = (1 - unit_cell_volume / general_volume) * 100;
   

    return (vec4 ( porosity, pore_size_A_A * 0.1, pore_size_B_B * 0.1,0));

}

vec4 comp_pores_45(float h,float l ,float t, float theta)
{
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
vec4 comp_pores(float h,float l ,float t, float theta, int type)
{
    if(type==0)
    {
        return (comp_pores_45(h,l,t,theta));
    }
    else if(type==1)
    {
        return (comp_pores_90(h,l,t,theta));
    }
}

bool check_limits(float h,float l ,float t, float theta)
{
    if (h < limits_h.x || h > limits_h.y)
    {
        return(false);
    }
    if (l < limits_l.x || l > limits_l.y)
    {
        return(false);
    }
    if (t < limits_t.x || t > limits_t.y)
    {
        return(false);
    }
    if (theta < limits_theta.x || theta > limits_theta.y)
    {
        return(false);
    }
    if(type_comp==1)
    {
        if (h-4*t-2*l*cos(radians(theta)) < 2*t)
        {
        return(false);
        }
    }
    return(true);
}

void main() 
{
    int porosity_q = int(float(sizeXY.z)*porosity_inp/100);
    if (porosity_q >= 0 && porosity_q <= sizeXY.z-1)
    {
        ivec2 ipos_pores_ind = ivec2(porosity_q,sizeXY.w-1);
        int ind_d = int(imageLoad(porosity_map, ipos_pores_ind).x);
        int ind_i = 0;
        for(int i=0; i < ind_d; i++)
        {
            ivec2 ipos_pores = ivec2(porosity_q,i);
            vec4 val = imageLoad(porosity_map, ipos_pores);
            vec4 ret = comp_pores(val.x, val.y,val.z,val.w,type_comp);

            float k = pore_size_inp/ret.y;

            float h = val.x*k;
            float l = val.y*k;
            float t = val.z*k;
            float theta = val.w;
            vec4 ret_n = vec4(h,l,t,theta);
            vec4 val_n = comp_pores(h,l,t,theta,type_comp);
            if(check_limits(h,l,t,theta))
            {
                imageStore(aux_data, ivec2(ind_i,0), ret_n);
                imageStore(aux_data, ivec2(ind_i,1), val_n);
                ind_i++;
            }    
        }
    }      
}	

