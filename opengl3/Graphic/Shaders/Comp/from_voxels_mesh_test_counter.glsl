#version 430 core

layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;
layout(std430, binding = 0) readonly buffer VoxelBuffer {
    uint voxels[];
};
layout(std430, binding = 3) buffer VertexCounter { uint counter; } vertexCounter;
uniform int pass;           // 0 = подсчет, 1 = запись
uniform int width;          // размер X (вокселей)
uniform int height;         // размер Y
uniform int depth;          // размер Z

uint getVoxel(int x, int y, int z) {
    if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth)
        return 0;
    int idx = x + y * width + z * width * height;
    return voxels[idx];
}

void main() {
    if (pass == 0 && gl_GlobalInvocationID.x == 23 && gl_GlobalInvocationID.y == 23 && gl_GlobalInvocationID.z == 25) 
    {
   // {

        uint val = getVoxel(23,23,25);
        atomicAdd(vertexCounter.counter, val + 1); // записываем 1, если val=0, и 2, если val=1
    
       // atomicAdd(vertexCounter.counter, width + height * 100 + depth * 10000);
   // }
    }
}