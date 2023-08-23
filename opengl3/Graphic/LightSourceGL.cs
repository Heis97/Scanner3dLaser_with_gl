using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace opengl3
{
    public class LightSourceGL
    {
        enum type { Point = 1, Direct = 2, Disc = 3};
        public Vertex3f direction;
        public Vertex3f position;
        public Vertex3f color;
        public Vertex3f settings;//(type,power,cut_off)
        public Matrix4x4f to_mat4()
        {
            return new Matrix4x4f(direction.x, direction.y, direction.z, 0,
                position.x, position.y, position.z, 0,
                color.x, color.y, color.z, 0,
                 settings.x, settings.y, settings.z, 0);

    }
    }

    
}
