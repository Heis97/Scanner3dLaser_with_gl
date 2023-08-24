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
        public enum type { Point = 1, Direct = 2, Disc = 3 }

        public float direction_x{ get; set;}
        public float direction_y{ get; set;}
        public float direction_z { get; set; }
        public float position_x { get; set; }
        public float position_y { get; set; }
        public float position_z { get; set; }
        public float color_r { get; set; }
        public float color_g { get; set; }
        public float color_b { get; set; }

        public type type_light { get; set; }
        public float power { get; set; }
        public float cut_off { get; set; }

        public Matrix4x4f to_mat4()
        {
            return new Matrix4x4f(direction_x, direction_y, direction_z, 0,
                position_x, position_y, position_z, 0,
                color_r, color_g, color_b, 0,
                 (float)type_light, power, cut_off, 0);

        }
    }
    public class LightSourcesGL
    {
        public LightSourceGL[] lightSources{ get; set; }
    }


}
