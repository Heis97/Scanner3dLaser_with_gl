#version 430 core


#define RADIUS 4
#define GROUP_SIZE 8
#define BLOCK_SIZE (GROUP_SIZE + 2 * RADIUS)  // = 20

shared int sharedVoxels[BLOCK_SIZE][BLOCK_SIZE][BLOCK_SIZE];

layout(std430, binding = 0) buffer VoxelInput {
    int voxels[];
} ;

layout(std430, binding = 1) buffer VoxelOutput {
    int outVoxels[];
};

layout(std430, binding = 2) buffer Parameters {
    int Params[];
};

/*uniform int u_width;
uniform int u_height;
uniform int u_depth;*/



layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;
void main() {

int u_width = Params[0];
int u_height = Params[1];
int u_depth = Params[2];
    
    int gx = int(gl_GlobalInvocationID.x);
    int gy = int(gl_GlobalInvocationID.y);
    int gz = int(gl_GlobalInvocationID.z);




    //if (gx >= u_width || gy >= u_height || gz >= u_depth) return;

    int localX = int(gl_LocalInvocationID.x);
    int localY = int(gl_LocalInvocationID.y);
    int localZ = int(gl_LocalInvocationID.z);

    int baseX = gx - RADIUS;
    int baseY = gy - RADIUS;
    int baseZ = gz - RADIUS;

    // Загрузка в shared-память
    /*for (int dz = 0; dz < BLOCK_SIZE; dz += GROUP_SIZE) {
        for (int dy = 0; dy < BLOCK_SIZE; dy += GROUP_SIZE) {
            for (int dx = 0; dx < BLOCK_SIZE; dx += GROUP_SIZE) {
                int lx = localX + dx;
                int ly = localY + dy;
                int lz = localZ + dz;
                if (lx < BLOCK_SIZE && ly < BLOCK_SIZE && lz < BLOCK_SIZE) {
                    int wx = baseX + lx;
                    int wy = baseY + ly;
                    int wz = baseZ + lz;
                    int value = 0;
                    if (wx >= 0 && wx < u_width && wy >= 0 && wy < u_height && wz >= 0 && wz < u_depth) {
                        int idx = wx + wy * u_width + wz * u_width * u_height;
                        value = voxels[idx];
                    }
                    sharedVoxels[lx][ly][lz] = value;
                }
            }
        }
    }*/

    for (int dz = 0; dz < BLOCK_SIZE; dz += GROUP_SIZE) {
        for (int dy = 0; dy < BLOCK_SIZE; dy += GROUP_SIZE) {
            for (int dx = 0; dx < BLOCK_SIZE; dx += GROUP_SIZE) {
                int lx = localX + dx;
                int ly = localY + dy;
                int lz = localZ + dz;
                if (lx < BLOCK_SIZE && ly < BLOCK_SIZE && lz < BLOCK_SIZE) {
                    int wx = baseX + lx;
                    int wy = baseY + ly;
                    int wz = baseZ + lz;
                    int value = 0;
                    if (wx >= 0 && wx < u_width && wy >= 0 && wy < u_height && wz >= 0 && wz < u_depth) {
                        int idx = wx + wy * u_width + wz * u_width * u_height;
                        value = voxels[idx];
                    }
                    sharedVoxels[lx][ly][lz] = value;
                }
            }
        }
    }
    barrier();

    int sum = 0;
    int count = BLOCK_SIZE * BLOCK_SIZE * BLOCK_SIZE; // всегда 20^3 = 8000, но можно пересчитать
    // Лучше пересчитать явно:
    count = 0;
    for (int dz = -RADIUS; dz <= RADIUS; ++dz) {
        for (int dy = -RADIUS; dy <= RADIUS; ++dy) {
            for (int dx = -RADIUS; dx <= RADIUS; ++dx) {
                int lx = localX + RADIUS + dx;
                int ly = localY + RADIUS + dy;
                int lz = localZ + RADIUS + dz;
                sum += sharedVoxels[lx][ly][lz];
                count++;
            }
        }
    }
    int avg = int(round(float(sum) / float(count)));

    int outIdx = gx + gy * u_width + gz * u_width * u_height;
    outVoxels[outIdx] = avg;

    
}