using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class PolarP2d
    {
        public double r, fi;
        public PolarP2d(double r, double fi)
        {
            this.r = r;
            this.fi = fi;
        }

        public CartP2d toCart()
        {
            return new CartP2d(r * Math.Cos(fi), r * Math.Sin(fi));
        }
        public static CartP2d[] toCartArr(PolarP2d[] ps)
        {
            var result = new CartP2d[ps.Length];
            for (int i = 0; i < ps.Length; i++) result[i] = ps[i].toCart();
            return result;
        }

        public static double[][] polar_to_data(PolarP2d[] ps)
        {
            double[][] result = new double[ps.Length][];
            for (int i = 0; i < ps.Length; i++)
            {
                result[i] = new double[] { ps[i].fi, ps[i].r };
            }
            return result;
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
            return new CartP2d(p1.x / k, p1.y / k);
        }
        public static CartP2d operator *(CartP2d p1, double k)
        {
            return new CartP2d(p1.x * k, p1.y * k);
        }
        public PolarP2d toPolar()
        {
            var d = Math.Sqrt(x * x + y * y);

            double fi;
            if (x > 0 && y >= 0) fi = Math.Atan(y / x);
            else if (x > 0 && y < 0) fi = Math.Atan(y / x) + 2 * Math.PI;
            else if (x < 0) fi = Math.Atan(y / x) + Math.PI;
            else if (x == 0 && y > 0) fi = Math.PI / 2;
            else if (x == 0 && y < 0) fi = 3 * Math.PI / 2;
            else fi = 0;

            return new PolarP2d(d, fi);
        }
        public static PolarP2d[] toPolarArr(CartP2d[] ps)
        {
            var result = new PolarP2d[ps.Length];
            for (int i = 0; i < ps.Length; i++) result[i] = ps[i].toPolar();
            return result;
        }
        public static CartP2d[] data_to_cart2_d(double[][] ps)
        {
            CartP2d[] result = new CartP2d[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                result[i] = new CartP2d(ps[i][0], ps[i][1]);
            }
            return result;
        }
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
            for (int i = 0; i < ps.Length; i++)
            {
                ps_al[i] = ps[i] - pc;
            }
            return ps_al;
        }


        public static CartP2d[] regress_polar(double[][] data)
        {
            var ps = CartP2d.data_to_cart2_d(data);
            var ps_al = allign(ps);
            var pl = CartP2d.toPolarArr(ps_al);
            var data_p = PolarP2d.polar_to_data(pl);


            return null;
        }
    }

}
    
