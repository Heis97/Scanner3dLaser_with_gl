using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Color3d_GL
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public Color3d_GL(float r = 0.1f, float g = 0.1f, float b = 0.1f, float a = 1f, float max = 1)
        {
            this.r = r/max;
            this.g = g / max;
            this.b = b / max;
            this.a = a;

        }

        public Color3d_GL(double r = 0.1f, double g = 0.1f, double b = 0.1f, double a = 1f,double max = 1)
        {
            this.r =(float) (r / max);
            this.g = (float)(g / max);
            this.b = (float)(b / max);
            this.a = (float)a;

        }

        public static Color3d_GL operator*(Color3d_GL color,double k)
        {
            return new Color3d_GL(color.r * k, color.g * k, color.b * k, color.a);
        }

        public static bool operator !=(Color3d_GL color1, Color3d_GL color2)
        {
            return (color1.r != color1.r) ||(color1.g != color1.g) || (color1.b != color1.b) || (color1.a != color1.a);
        }
        public static bool operator ==(Color3d_GL color1, Color3d_GL color2)
        {
            return (color1.r == color1.r) && (color1.g == color1.g) || (color1.b == color1.b) || (color1.a == color1.a);
        }
        public static Color3d_GL black() { return new Color3d_GL(0, 0, 0); }
        public static Color3d_GL red() { return new Color3d_GL(1, 0, 0); }
        public static Color3d_GL green() { return new Color3d_GL(0, 1, 0); }
        public static Color3d_GL blue() { return new Color3d_GL(0, 0, 1); }

        public static Color3d_GL purple() { return new Color3d_GL(1, 0, 1); }
        public static Color3d_GL yellow() { return new Color3d_GL(1, 1, 0); }

        public static Color3d_GL aqua() { return new Color3d_GL(0, 1, 1); }

        public static Color3d_GL white() { return new Color3d_GL(1, 1, 1); }
        public static Color3d_GL gray() { return new Color3d_GL(0.5, 0.5, 0.5); }
        static public Color3d_GL random()
        {
            var rand = Accord.Math.Random.Generator.Random;
            var r = (float)rand.NextDouble();
            var g = (float)rand.NextDouble();
            var b = (float)rand.NextDouble();
            //Console.WriteLine(r + " " + g + " " + b);
            return new Color3d_GL(b, g, r);
        }

        public Color3d_GL set_r(double r)
        {
            this.r = (float)r;
            return this;
        }

        public override string ToString()
        {
            return r + " " + g + " " + b + " " + a;
        }

    }
}
