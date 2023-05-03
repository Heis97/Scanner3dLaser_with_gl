using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace opengl3
{
    public enum Ax { X,Y,Z};
    static class SurfaceReconstraction
    {
        
        static public Point3d_GL[] order_ps_by_ax(Point3d_GL[] ps,Ax ax)
        {
            var ps_or = new Point3d_GL[ps.Length];
            if(ax == Ax.X)
            {
                ps_or = (from p in ps
                         orderby p.x
                         select p).ToArray();
            }
            if (ax == Ax.Y)
            {
                ps_or = (from p in ps
                         orderby p.y
                         select p).ToArray();
            }
            if (ax == Ax.Z)
            {
                ps_or = (from p in ps
                         orderby p.z
                         select p).ToArray();
            }
            return ps_or;
        }

        static public double[][] prep_for_regr(Point3d_GL[] ps, Ax ax_X, Ax ax_Y)
        {
            var data = new List<double[]>();
            for(int i = 0; i < ps.Length; i++)
            {
                var v_X = get_val_ax(ps, i, ax_X);
                var v_Y = get_val_ax(ps, i, ax_Y);
                data.Add(new double[] { v_X, v_Y });
            }
            return data.ToArray();
        }

        static public Point3d_GL[] reconstruct(Point3d_GL[] ps, Ax ax_X, Ax ax_Y, Ax const_ax, double dx)
        {
            var ps_or = order_ps_by_ax(ps, ax_X);
            var data = prep_for_regr(ps_or, ax_X, ax_Y);
            var koef = Regression.regression(data, 3);
            double v_X_min = get_val_ax(ps_or, 0, ax_X);
            double v_X_max = get_val_ax(ps_or, ps_or.Length - 1, ax_X);

            var ps_rec = new List<Point3d_GL>();
            var const_v = get_val_ax(ps, 0, const_ax);
            for (double i = v_X_min; i < v_X_max; i+= Math.Abs(dx))
            {
                var p = new Point3d_GL(0,0,0);
                var v = Regression.calcPolynSolv(koef, i);
                p = set_val_ax(p, i, ax_X);
                p = set_val_ax(p, v, ax_Y);
                p = set_val_ax(p, const_v, const_ax);
                ps_rec.Add(p);
            }
            return ps_rec.ToArray();
        }
        static public Point3d_GL[] reconstruct_3d(Point3d_GL[] ps, Ax ax_X, double dx)
        {

            // var ps_rec = Regression.regress3DLine(ps, )

            //return ps_rec.ToArray();
            return null;
        }

        static double get_val_ax(Point3d_GL[] ps, int ind, Ax ax1)
        {
            double v = 0;
            switch (ax1)
            {
                case Ax.X: v = ps[ind].x; break;
                case Ax.Y: v = ps[ind].y; break;
                case Ax.Z: v = ps[ind].z; break;
            }
            return v;
        }
        static Point3d_GL set_val_ax(Point3d_GL p, double v,  Ax ax1)
        {
            switch (ax1)
            {
                case Ax.X: p.x = v; break;
                case Ax.Y: p.y = v; break;
                case Ax.Z: p.z = v; break;
            }
            return p;
        }

        /*static Polygon3d_GL[] splines_to_mesh(CubicSpline[] splines)
        {


            return null;
        }*/



       
    }
}
