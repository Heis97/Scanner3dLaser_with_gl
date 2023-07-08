using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class Color3d_GL
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public Color3d_GL(float r = 0.1f, float g = 0.1f, float b = 0.1f, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public static Color3d_GL black() { return new Color3d_GL(0, 0, 0); }
        public static Color3d_GL red() { return new Color3d_GL(1, 0, 0); }
        public static Color3d_GL green() { return new Color3d_GL(0, 1, 0); }
        public static Color3d_GL blue() { return new Color3d_GL(0, 0, 1); }

        public static Color3d_GL purple() { return new Color3d_GL(1, 0, 1); }
        public static Color3d_GL yellow() { return new Color3d_GL(1, 1, 0); }

        public static Color3d_GL aqua() { return new Color3d_GL(0, 1, 1); }


    }
}
