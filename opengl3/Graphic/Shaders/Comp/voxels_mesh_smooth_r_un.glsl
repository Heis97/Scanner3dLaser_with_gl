#version 430 core
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

layout(std430, binding = 0) readonly buffer VoxelInput {
    int voxels[];
};

layout(std430, binding = 1) writeonly buffer VoxelOutput {
    int outVoxels[];
} ;
layout(std430, binding = 2) buffer Parameters {
    int Params[];
};


void main() {

    int u_width = Params[0];
    int u_height = Params[1];
    int u_depth = Params[2];
    int u_windowSize = Params[3];

    int x = int(gl_GlobalInvocationID.x);
    int y = int(gl_GlobalInvocationID.y);
    int z = int(gl_GlobalInvocationID.z);
    if (x >= u_width || y >= u_height || z >= u_depth) return;
    int outIdx = x + y * u_width + z * u_width * u_height;
    if(x==0 || y==0|| z==0 || x==u_width-1|| y==u_height-1|| z==u_depth-1) {outVoxels[outIdx] = 0 ;return; }
    int radius = u_windowSize / 2;
    int sum = 0;
    int count = 0;
    for (int dz = -radius; dz <= radius; ++dz) {
        int wz = z + dz;
        if (wz < 0 || wz >= u_depth) continue;
        for (int dy = -radius; dy <= radius; ++dy) {
            int wy = y + dy;
            if (wy < 0 || wy >= u_height) continue;
            for (int dx = -radius; dx <= radius; ++dx) {
                int wx = x + dx;
                if (wx < 0 || wx >= u_width) continue;
                int idx = wx + wy * u_width + wz * u_width * u_height;
                sum += voxels[idx];
                ++count;
            }
        }
    }
    int avg = int(round(float(sum) / float(count)));
    
    outVoxels[outIdx] = avg;
    

}