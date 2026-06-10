#version 430 core
//layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;
// ---------------------------- Входные данные ----------------------------
// Воксельные данные: упакованы в одномерный массив uint (0 или 1)
layout(std430, binding = 0) readonly buffer VoxelBuffer {
    uint voxels[];
};

// ---------------------------- Выходные данные ----------------------------
layout(std430, binding = 1) buffer VertexBuffer {
    vec3 vertices[];
};

layout(std430, binding = 6) buffer IndexBuffer {
    uint indices[];
};
// ---------------------------- Счетчики (атомарные) ----------------------------
//layout(binding = 3) uniform atomic_uint vertexCounter;
layout(std430, binding = 3) buffer VertexCounter { uint counter; } vertexCounter;
layout(std430, binding = 4) buffer IndexCounter { uint counter; } indexCounter;
//layout(binding = 4) uniform atomic_uint indexCounter;
layout(std430, binding = 5) readonly buffer TriTable {
    int triTable[4096];
};

/*layout(std430, binding = 6) buffer DebugTable {
    uint debugTable[];
};*/


// ---------------------------- Uniform-параметры ----------------------------
uniform int pass;           // 0 = подсчет, 1 = запись
uniform int width;          // размер X (вокселей)
uniform int height;         // размер Y
uniform int depth;          // размер Z
uniform float isoLevel = 127;  // уровень изоповерхности (для бинарных вокселов = 0.5)

// ---------------------------- Таблицы Marching Cubes ----------------------------
// Таблица рёбер: для каждого индекса куба (0..255) – 12-битная маска, какие рёбра пересекаются

const int edgeTable[256] = int[256](
    0x000, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c, 0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
    0x190, 0x099, 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c, 0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
    0x230, 0x339, 0x033, 0x13a, 0x636, 0x73f, 0x435, 0x53c, 0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
    0x3a0, 0x2a9, 0x1a3, 0x0aa, 0x7a6, 0x6af, 0x5a5, 0x4ac, 0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
    0x460, 0x569, 0x663, 0x76a, 0x066, 0x16f, 0x265, 0x36c, 0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
    0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0x0ff, 0x3f5, 0x2fc, 0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
    0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x055, 0x15c, 0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
    0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0x0cc, 0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
    0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc, 0x0cc, 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
    0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c, 0x15c, 0x055, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
    0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc, 0x2fc, 0x3f5, 0x0ff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
    0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c, 0x36c, 0x265, 0x16f, 0x066, 0x76a, 0x663, 0x569, 0x460,
    0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac, 0x4ac, 0x5a5, 0x6af, 0x7a6, 0x0aa, 0x1a3, 0x2a9, 0x3a0,
    0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c, 0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x033, 0x339, 0x230,
    0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c, 0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x099, 0x190,
    0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c, 0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x000
);

// Таблица треугольников: для каждого индекса куба – до 5 треугольников по 3 индекса рёбер

// ---------------------------- Вспомогательные функции ----------------------------
// Получение значения вокселя по координатам (x, y, z) в диапазоне [0..size-1]
uint getVoxel(int x, int y, int z) {
    if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth)
        return 0;
    int idx = x + y * width + z * width * height;
    return voxels[idx];
}

/*void setVoxel_debug(int x, int y, int z,uint val) {
    int idx = x + y * width + z * width * height;
    debugTable[idx] = val;
}*/
// Получение позиции вершины куба (0..7) в мировых координатах (целые индексы вокселей)

// Интерполяция точки на ребре между двумя вершинами v1 и v2
// val1, val2 – значения вокселей (0 или 1). Для isoLevel=0.5 даёт среднюю точку.
/*vec3 interpolate(vec3 p1, vec3 p2, float val1, float val2) {
   // return (p2 + p1)/2;
    float t = (isoLevel - val1) / (val2 - val1);
    return p1 + t * (p2 - p1);
}*/

vec3 interpolate(vec3 p1, vec3 p2, float v1, float v2) {
    //return (p2 + p1)/2;
    float t = (isoLevel - v1) / (v2 - v1);
    return mix(p1, p2, t);
}

// ---------------------------- Основная функция шейдера ----------------------------
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;
void main() {
    
    // Глобальный ID куба (в воксельных единицах)
    ivec3 cubePos = ivec3(gl_GlobalInvocationID.xyz);
    int x = cubePos.x;
    int y = cubePos.y;
    int z = cubePos.z;
    

  
    // Проверка границ: обрабатываем кубы только внутри сетки (последний воксель по каждому измерению не может быть началом куба)
    //if (x >= width - 1 || y >= height - 1 || z >= depth - 1)
        //return;
    
    // Чтение значений 8 вершин куба
    uint v[8];
    v[0] = getVoxel(x,   y,   z);
    v[1] = getVoxel(x+1, y,   z);
    v[2] = getVoxel(x+1, y,   z+1);
    v[3] = getVoxel(x,   y,   z+1);
    v[4] = getVoxel(x,   y+1, z);
    v[5] = getVoxel(x+1, y+1, z);
    v[6] = getVoxel(x+1, y+1, z+1);
    v[7] = getVoxel(x,   y+1, z+1);
    
    // Вычисление индекса куба (битовая маска вершин, превышающих isoLevel)
    int cubeIndex = 0;
    for (int i = 0; i < 8; i++) {
        if (float(v[i]) > isoLevel)
            cubeIndex |= (1 << i);
    }
    
    // Если куб полностью внутри или снаружи – выходим
    if (edgeTable[cubeIndex] == 0)
        return;
    
    
    // Вычисление 12 точек пересечения на рёбрах (если ребро пересекается)
    
    // Рёбра: стандартная нумерация Marching Cubes (см. edgeTable)
    // 0: (0,1), 1: (1,2), 2: (2,3), 3: (3,0), 4: (4,5), 5: (5,6), 6: (6,7), 7: (7,4), 8: (0,4), 9: (1,5), 10: (2,6), 11: (3,7)

    
    vec3 vertList[12] = vec3[12](vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0), vec3(0.0));
    if ((edgeTable[cubeIndex] & 1) != 0) vertList[0] = interpolate(vec3(x,y,z), vec3( x+1,y,z), float(v[0]), float(v[1]));
    if ((edgeTable[cubeIndex] & 2) != 0) vertList[1] = interpolate(vec3(x+1,y,z), vec3( x+1,y,z+1), float(v[1]), float(v[2]));
    if ((edgeTable[cubeIndex] & 4) != 0) vertList[2] = interpolate(vec3(x+1,y,z+1), vec3( x,y,z+1), float(v[2]), float(v[3]));
    if ((edgeTable[cubeIndex] & 8) != 0) vertList[3] = interpolate(vec3(x,y,z+1), vec3( x,y,z), float(v[3]), float(v[0]));

    if ((edgeTable[cubeIndex] & 16) != 0) vertList[4] = interpolate(vec3(x,y+1,z), vec3( x+1,y+1,z), float(v[4]), float(v[5]));
    if ((edgeTable[cubeIndex] & 32) != 0) vertList[5] = interpolate(vec3(x+1,y+1,z), vec3( x+1,y+1,z+1), float(v[5]), float(v[6]));
    if ((edgeTable[cubeIndex] & 64) != 0) vertList[6] = interpolate(vec3(x+1,y+1,z+1), vec3( x,y+1,z+1), float(v[6]), float(v[7]));
    if ((edgeTable[cubeIndex] & 128) != 0) vertList[7] = interpolate(vec3(x,y+1,z+1), vec3( x,y+1,z), float(v[7]), float(v[4]));

    if ((edgeTable[cubeIndex] & 256) != 0) vertList[8] = interpolate(vec3(x,y,z), vec3( x,y+1,z), float(v[0]), float(v[4]));
    if ((edgeTable[cubeIndex] & 512) != 0) vertList[9] = interpolate(vec3(x+1,y,z), vec3( x+1,y+1,z), float(v[1]), float(v[5]));
    if ((edgeTable[cubeIndex] & 1024) != 0) vertList[10] = interpolate(vec3(x+1,y,z+1), vec3( x+1,y+1,z+1), float(v[2]), float(v[6]));
    if ((edgeTable[cubeIndex] & 2048) != 0) vertList[11] = interpolate(vec3(x,y,z+1), vec3( x,y+1,z+1), float(v[3]), float(v[7]));

    //setVoxel_debug(x,   y,   z,v[0]);
    // Подсчёт количества треугольников для этого куба
    int numTriangles = 0;
    int base = cubeIndex * 16;
    for (int i = 0; i < 5; i++) {
        int i0 = triTable[base + i*3];
        if (i0 == -1) break;
        numTriangles++;
    }
    

     //if(pass==0)   
    if (pass == 0) {
        // Первый проход: только подсчёт вершин и индексов
        uint vertexCount = numTriangles * 3;
        atomicAdd(vertexCounter.counter, vertexCount);
        atomicAdd(indexCounter.counter, vertexCount);

        
        
    } 
    else {
        // Второй проход: запись вершин и индексов в выходные буферы
        // Получаем глобальные смещения для записи
        uint baseVertex = atomicAdd(vertexCounter.counter, numTriangles * 3);  
        uint baseIndex = atomicAdd(indexCounter.counter, numTriangles * 3);  
        
        // Для каждого треугольника
        for (int tri = 0; tri < numTriangles; tri++) {
            int i0 = triTable[base + tri*3];

            if (i0 == -1) break;
            int i1 = triTable[base + tri*3 + 1];
            int i2 = triTable[base + tri*3 + 2];

            vec3 v0 = vertList[i0];
            vec3 v1 = vertList[i1];
            vec3 v2 = vertList[i2];
            
            // Запись вершин
            vertices[baseVertex + tri*3 + 0] = v0;
            vertices[baseVertex + tri*3 + 1] = v1;
            vertices[baseVertex + tri*3 + 2] = v2;
            
            // Запись индексов (просто последовательные)
            indices[baseIndex + tri*3 + 0] = baseVertex + tri*3 + 0;
            indices[baseIndex + tri*3 + 1] = baseVertex + tri*3 + 1;
            indices[baseIndex + tri*3 + 2] = baseVertex + tri*3 + 2;
        }
    }
}