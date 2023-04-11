using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class PolarP2d
    {
        public double d, fi;
        public PolarP2d(double d, double fi)
        {
            this.d = d;
            this.fi = fi;
        }

    }

    public class CartP2d
    {
        public double x, y;
        public CartP2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public static CartP2d operator +(CartP2d p1, CartP2d p2)
        {
            return new CartP2d(p1.x + p2.x, p1.y + p2.y);
        }
        public static CartP2d operator -(CartP2d p1, CartP2d p2)
        {
            return new CartP2d(p1.x - p2.x, p1.y - p2.y);
        }
        public static CartP2d operator /(CartP2d p1, double k)
        {
            return new CartP2d(p1.x / k, p1.y /k);
        }
        public static CartP2d operator *(CartP2d p1, double k)
        {
            return new CartP2d(p1.x * k, p1.y * k);
        }
       /* public static PolarP2d toPolar()
        {

        }*/

    }
    static public class Polar2d
    {
        public static CartP2d mass_centr(CartP2d[] ps)
        {
            var pc = new CartP2d(0, 0);
            foreach (CartP2d c in ps)
            {
                pc += c;
            }
            return pc / ps.Length;
        }
        public static CartP2d[] allign(CartP2d[] ps)
        {
            var pc = mass_centr(ps);
            var ps_al = new CartP2d[ps.Length];
            for(int i = 0; i < ps.Length; i++)
            {
                ps_al[i] = ps[i] - pc;
            }
            return ps_al;
        }

    }
}
