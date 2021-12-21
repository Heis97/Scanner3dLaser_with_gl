using Accord.Math;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public enum PlanType { X, Y, XY }
    public enum DirectionType { Up , Down }
    public enum SolveType { Simple, Complex }
    public enum FrameType { Pos, Las, Test,MarkBoard,Stereo }

    public class Camera
    {
        public Point3d_GL pos;
        public Vector3d_GL oX;
        public Vector3d_GL oY;
        public Vector3d_GL oZ;
        public List<Vector3d_GL> Points;
        public double err_pos;
        public double fov;
        public double alfa_ch_x;
        public double alfa_ch_y;
        public double ang_ch_xy;
        public double dim;
        public double f;
        public double[,] robToCam;
        private double[] r;
        public int[] sign;
        private Size size;
        public Camera()
        {
            Points = new List<Vector3d_GL>();
            r = new double[3];
            sign = new int[10] { 1, 1, 1, 1, 1, 1, 0, 0, 1, 1 };
        }
        public Camera(double _fov, Size _size)
        {
            size = _size;
            Points = new List<Vector3d_GL>();
            r = new double[3];
            sign = new int[10] { 1, 1, 1, 1, 1, 1, 0, 0, 1, 1 };
            calc_f(_fov, size.Width);
            robToCam = null;
        }
        public double calc_f(double _fov, double _dim)
        {
            f = _dim / (2 * Math.Tan(DegreesToRad(_fov) / 2));
            Console.WriteLine("F " + f);
            return f;
        }
        private double DegreesToRad(double degr)
        {
            return (Math.PI * degr) / 180;
        }
        private double RadToDegrees(double rad)
        {
            return (180 * rad) / Math.PI;
        }
        private double[] calcCirc(Point3d_GL p1, Point3d_GL p2, Point3d_GL p3)
        {
            double mx1 = p1.x;
            double my1 = p1.y;
            double mx2 = p2.x;
            double my2 = p2.y;
            double mx3 = p3.x;
            double my3 = p3.y;
            var x_okr = ((2 * my3 - 2 * my2) * (my2 * my2 - my1 * my1 + mx2 * mx2 - mx1 * mx1) - (2 * my2 - 2 * my1) * (my3 * my3 - my2 * my2 + mx3 * mx3 - mx2 * mx2)) / ((2 * my2 - 2 * my1) * (2 * mx2 - 2 * mx3) - (2 * my3 - 2 * my2) * (2 * mx1 - 2 * mx2));
            var y_okr = (my2 * my2 - my1 * my1 + mx2 * mx2 - mx1 * mx1 + 2 * mx1 * x_okr - 2 * mx2 * x_okr) / (2 * my2 - 2 * my1);
            var r_okr = Math.Sqrt(Math.Pow((my1 - y_okr), 2) + Math.Pow((mx1 - x_okr), 2));
            double[] ret = { x_okr, y_okr, r_okr };
            return ret;
        }
        private Point3d_GL calcPosRob(List<Frame> frames, int[] indexes)
        {
            var P1 = frames[indexes[0]].pos_cam;
            var P2 = frames[indexes[1]].pos_cam;
            var P3 = frames[indexes[2]].pos_cam;
            var ret = calcCirc(P1, P2, P3);
            return new Point3d_GL(ret[0], ret[1], 0);
        }
        public Point3d_GL calc_sr_p3(Point3d_GL P1, Point3d_GL P2)
        {
            var dP = P2 - P1;
            dP *= 0.5;
            P2 += dP;
            return P2;
        }
        public void addPointF(Point p1)
        {
            var P1_cam = new Vector3d_GL(p1, f);

            var P1_x = oX * P1_cam.x;
            var P1_y = oY * P1_cam.y;//*-1
            var P1_z = oZ * P1_cam.z;
            var P1_base = P1_x + P1_y + P1_z;
            P1_base.normalize();
            if (Points != null)
            {
                Points.Add(P1_base);
            }
            else
            {
                Console.WriteLine("Points = null");
            }
        }
        public Flat3d_GL calc_flat(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
        {
            double D = P1.x * P2.y * P3.z - P1.x * P3.y * P2.z - P2.x * P1.y * P3.z + P2.x * P3.y * P1.z + P3.x * P1.y * P2.z - P3.x * P2.y * P1.z;
            double A = -(P1.y * P2.z - P2.y * P1.z - P1.y * P3.z + P3.y * P1.z + P2.y * P3.z - P3.y * P2.z);
            double B = (P1.x * P2.z - P2.x * P1.z - P1.x * P3.z + P3.x * P1.z + P2.x * P3.z - P3.x * P2.z);
            double C = -(P1.x * P2.y - P2.x * P1.y - P1.x * P3.y + P3.x * P1.y + P2.x * P3.y - P3.x * P2.y);
            return new Flat3d_GL(A, B, C, D);
        }
        public Point3d_GL calc_p_3flat(Flat3d_GL F1, Flat3d_GL F2, Flat3d_GL F3)
        {
            var A = F1.A;
            var B = F1.B;
            var C = F1.C;
            var D = F1.D;
            var A1 = F2.A;
            var B1 = F2.B;
            var C1 = F2.C;
            var D1 = F2.D;
            var A2 = F3.A;
            var B2 = F3.B;
            var C2 = F3.C;
            var D2 = F3.D;
            var d = A * B1 * C2 - A * B2 * C1 - A1 * B * C2 + A1 * B2 * C + A2 * B * C1 - A2 * B1 * C;
            var x = -(B * C1 * D2 - B1 * C * D2 - B * C2 * D1 + B2 * C * D1 + B1 * C2 * D - B2 * C1 * D) / d;
            var y = (A * C1 * D2 - A1 * C * D2 - A * C2 * D1 + A2 * C * D1 + A1 * C2 * D - A2 * C1 * D) / d;
            var z = -(A * B1 * D2 - A1 * B * D2 - A * B2 * D1 + A2 * B * D1 + A1 * B2 * D - A2 * B1 * D) / d;
            return new Point3d_GL(x, y, z);
        }
        private double calc_alfa(PointF p1)
        {
            return Math.Cos(Math.Atan(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y) / f));
        }
        private double calc_p_len(Point p1)
        {

            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
        }
        private double calc_p_len(PointF p1)
        {

            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
        }
        private Vector3d_GL calc_oZ(Vector3d_GL V1, Vector3d_GL V2, PointF p1, PointF p2, int sign)
        {
            var alfa1 = Math.Abs(calc_alfa(p1));
            var alfa2 = Math.Abs(calc_alfa(p2));
            //Console.WriteLine(" oZ_alfa1 = " + alfa1 + " oZ_alfa2 = " + alfa2);
            var v1 = V1.norm;
            var v2 = V2.norm;
            var x2 = V2.x;
            var y2 = V2.y;
            var z2 = V2.z;
            var x1 = V1.x;
            var y1 = V1.y;
            var z1 = V1.z;
            var kz = alfa1 * v1 * y2 - alfa2 * v2 * V1.y;
            var ky = alfa1 * v1 * z2 - alfa2 * v2 * V1.z;
            var kx = alfa1 * v1 * x2 - alfa2 * v2 * V1.x;

            var m = -
                (
                   kz * kz * (
                      (2 * alfa2 * alfa2 * kx * ky * v2 * v2 - 2 * kz * kz * x2 * z2 - 2 * kx * ky * y2 * y2 + 2 * ky * kz * x2 * y2 + 2 * kx * kz * y2 * z2)
                      / (2 * kz * kz)
                              + sign * (alfa2 * v2 * (
                                    Math.Sqrt(kx * kx * y2 * y2 - alfa2 * alfa2 * ky * ky * v2 * v2 - alfa2 * alfa2 * kz * kz * v2 * v2 - alfa2 * alfa2 * kx * kx * v2 * v2
                                    + kx * kx * z2 * z2 - 2 * kx * ky * x2 * z2 - 2 * kx * kz * x2 * y2 + ky * ky * x2 * x2 + ky * ky * y2 * y2 - 2 * ky * kz * y2 * z2 + kz * kz * x2 * x2 + kz * kz * z2 * z2))
                                    ) / (kz)
                    )
                    /
               (alfa2 * alfa2 * kx * kx * v2 * v2 + alfa2 * alfa2 * kz * kz * v2 * v2 - kx * kx * y2 * y2 + 2 * kx * kz * x2 * y2 - kz * kz * x2 * x2)
                );
            var n = -(ky + m * kx) / kz;
            var check1 = (x1 * m + y1 * n + z1) / (Math.Sqrt(x1 * x1 + y1 * y1 + z1 * z1) * Math.Sqrt(m * m + n * n + 1));
            var check2 = (x2 * m + y2 * n + z2) / (Math.Sqrt(x2 * x2 + y2 * y2 + z2 * z2) * Math.Sqrt(m * m + n * n + 1));
            //Console.WriteLine(" oZ_ch1 = " + check1 + " oZ_ch2 = " + check2);
            return new Vector3d_GL(m, n, 1);
        }
        private Vector3d_GL calc_oXy(Vector3d_GL V1, PointF p1, PlanType pt)
        {
            int sign1 = 1;
            int sign2 = 1;
            int sign3 = 1;
            var alfa = V1 ^ oZ;
            var k = oZ.norm / (Math.Cos(alfa) * V1.norm);

            var V1_ = V1 * k;
            var B = V1_ - oZ;
            var x1 = oZ.x;
            var y1 = oZ.y;
            var z1 = oZ.z;
            var x2 = B.x;
            var y2 = B.y;
            var z2 = B.z;
            var v1 = oZ.norm;
            var v2 = B.norm;

            /* Console.WriteLine("alfa       " + plan + " " + Math.Cos(alfa) +
                 "\nk       " + k +
                 "\nx1       " + x1 +
                 "\ny1       " + y1 +
                 "\nz1       " + z1 +
                 "\nx2       " + x2 +
                 "\ny2       " + y2 +
                 "\nz2       " + z2);*/
            double axe;
            if (pt == PlanType.X)
            {
                axe = p1.X;
                sign1 = sign[0];
                sign2 = sign[1];
                sign3 = sign[2];
            }
            else
            {
                axe = p1.Y;
                sign1 = sign[3];
                sign2 = sign[4];
                sign3 = sign[5];
            }
            var a1 = sign2 * axe / calc_p_len(p1);
            //Console.WriteLine("alfa        " + Math.Acos(a1));
            var v1s2 = v1 * v1;
            var v2s2 = v2 * v2;
            var x1s2 = x1 * x1;
            var x2s2 = x2 * x2;
            var y1s2 = y1 * y1;
            var y2s2 = y2 * y2;
            var z1s2 = z1 * z1;
            var z2s2 = z2 * z2;
            var a1s2 = a1 * a1;
            var n = (x2s2 * y1 * z1 + x1s2 * y2 * z2 - a1s2 * v2s2 * y1 * z1 - x1 * x2 * y1 * z2 - x1 * x2 * y2 * z1
                + a1 * v2 * x1 * sign1 *
                Math.Sqrt(x1s2 * y2s2 - a1s2 * v2s2 * y1s2 - a1s2 * v2s2 * z1s2 - a1s2 * v2s2 * x1s2 +
                x1s2 * z2s2 - 2 * x1 * x2 * y1 * y2 - 2 * x1 * x2 * z1 * z2 + x2s2 * y1s2 + x2s2 * z1s2 + y1s2 * z2s2
                - 2 * y1 * y2 * z1 * z2 + y2s2 * z1s2))
                / (a1s2 * v2s2 * x1s2 + a1s2 * v2s2 * y1s2 - x1s2 * y2s2 + 2 * x1 * x2 * y1 * y2 - x2s2 * y1s2);
            var m = -((z1 + n * y1) / (x1));
            var ret = new Vector3d_GL(m, n, 1);
            alfa = ret ^ B;
            //Console.WriteLine("alfa_ch     " + (Math.Cos(alfa)- a1));
            if (pt == PlanType.X)
            {
                alfa_ch_x = (Math.Cos(alfa) - a1);
            }
            else
            {
                alfa_ch_y = (Math.Cos(alfa) - a1);
            }

            ret *= sign3;
            return ret;
        }
        bool solvCond(double[] ret)
        {
            return ret[1] > 0 && ret[2] > 0;
        }
        double findOneVar(double[] consts, double maxVal, Func<double, double[], double[]> func, Func<double[], bool> cond)
        {
            //ret of func - error
            double min_solv = 10000;
            double solv = 0;
            double eps = 0.01;
            var step = 0.1;
            int ind = 0;
            for (double i = 0.5; i < maxVal && (min_solv > eps); i += step)
            {

                var ret = func(i, consts);
                step = Math.Abs(ret[0] / 500);
                if (cond(ret))
                {
                    if (Math.Abs(ret[0]) < Math.Abs(min_solv))
                    {
                        solv = i;
                        min_solv = Math.Abs(ret[0]);
                    }
                }
                //ind++;
            }
            //Console.WriteLine("ind: " + ind);
            return solv;
        }
        /* double findOneVarDihotomy(double[] consts, double maxVal, Func<double, double[], double[]> func, Func<double[], bool> cond)
         {
             double epsilon = 1;
             var a = 0.0;
             var b = maxVal;
             var c = 0.0;
             int i = 0;

             while (b - a > epsilon && i<1000)
             {
                 i++;
                 c = (a + b) / 2;
                 if (Math.Abs(func(b,consts)[0]) <Math.Abs(func(c, consts)[0] ))
                     a = c;
                 else
                     b = c;
                 Console.WriteLine(func(c, consts)[0]);
             } ;

             return (a + b) / 2;
         }*/
        private double[] calc_solv(double r2, double[] _in)
        {
            double[] vec = new double[3];
            var d1 = _in[0];
            var d2 = _in[1];
            var d3 = _in[2];
            var k1 = _in[3];
            var k2 = _in[4];
            var k3 = _in[5];
            var dir1 = _in[6];
            var dir2 = _in[7];
            var dir3 = _in[8];
            double r1 = dir1*Math.Sqrt(k1 * k1 * r2 * r2 - r2 * r2 + d1 * d1) + k1 * r2;
            double r3 = dir3* Math.Sqrt(k2 * k2 * r2 * r2 - r2 * r2 + d2 * d2) + k2 * r2;
            double solv = r1 * r1 + r3 * r3 - 2 * r1 * r3 * k3 - d3 * d3;
            
            vec[0] = dir2*solv;
            vec[1] = r1;
            vec[2] = r3;
            return vec;
        }
        private Point3d_GL trialat(Point3d_GL p1, Point3d_GL p2, Point3d_GL p3, double r1, double r2, double r3)
        {
            if(p2.x!=0)
            {
                double U = p2.x;
                double Vx = p3.x;
                double Vy = p3.y;
                double V2 = Vx * Vx + Vy * Vy;
                double x = (r1 * r1 - r2 * r2 + U * U) / (2 * U);
                double y = (r1 * r1 - r3 * r3 + V2 - 2 * Vx * x) / (2 * Vy);
                double z = Math.Sqrt(r1 * r1 - x * x - y * y);
                return new Point3d_GL(x, y, z);
            }
            else
            {
                double U = p2.y;
                double Vx = p3.y;
                double Vy = p3.x;
                double V2 = Vx * Vx + Vy * Vy;
                double x = (r1 * r1 - r2 * r2 + U * U) / (2 * U);
                double y = (r1 * r1 - r3 * r3 + V2 - 2 * Vx * x) / (2 * Vy);
                double z = Math.Sqrt(r1 * r1 - x * x - y * y);
                return new Point3d_GL(y, x, z);
            }           
        }
        private double calcRasst(Point3d_GL p1, Point3d_GL p2)
        {
            return Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z));
        }
        private double findCosAlpha(PointF p1, PointF p2)
        {
            var P1 = new Point3d_GL(0, 0, -f);
            var P2 = new Point3d_GL(p1, 0);
            var P3 = new Point3d_GL(p2, 0);
            var r1 = calcRasst(P1, P2);
            var r2 = calcRasst(P2, P3);
            var r3 = calcRasst(P3, P1);
            return (-(r2 * r2) + r1 * r1 + r3 * r3) / (2 * r1 * r3);
        }
        public void calc_pos(PointF p1, PointF p2, PointF p3,
            Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
        {
            var d1 = calcRasst(P1, P2);
            var d2 = calcRasst(P2, P3);
            var d3 = calcRasst(P3, P1);

            var k1 = findCosAlpha(p1, p2);
            var k2 = findCosAlpha(p2, p3);
            var k3 = findCosAlpha(p3, p1);
            int dir1 = 1;
            int dir2 = 1;
            int dir3 = 1;
            if (p1.X>p3.X)
            {

                dir1 = 1;
                dir2 = 1;
                dir3 = -1;
               // Console.WriteLine("right");
            }
            else
            {
                dir1 = -1;
                dir2 = 1;
                dir3 = 1;
                //Console.WriteLine("left");
            }
            
            var _in = new double[] { d1, d2, d3, k1, k2, k3,dir1,dir2,dir3 };
            double solv_max = 400;
            var r2_t = findOneVar(_in, solv_max, calc_solv, solvCond);
            var solv = calc_solv(r2_t, _in);
            var r1 = solv[1];
            var r2 = r2_t;
            var r3 = solv[2];
            pos = trialat(P1, P2, P3, r1, r2, r3);

            var V1 = new Vector3d_GL(P1, pos);
            var V2 = new Vector3d_GL(P2, pos);
            var V3 = new Vector3d_GL(P3, pos);
            var V4 = new Vector3d_GL(P1, pos);
            Vector3d_GL[] v_mass = new Vector3d_GL[4] { V1, V2, V3, V4 };
            PointF[] p_mass = new PointF[3] { p1, p2, p3 };
            calcOrient(v_mass, p_mass);

            
            oZ.normalize();
        }
        Point3d_GL[] calc_pos_simple(PointF p1, PointF p2, PointF p3,
            Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
        {
            var d1 = calcRasst(P1, P2);
            var d2 = calcRasst(P2, P3);
            var d3 = calcRasst(P3, P1);

            var k1 = findCosAlpha(p1, p2);
            var k2 = findCosAlpha(p2, p3);
            var k3 = findCosAlpha(p3, p1);

            var pos_all = new List<Point3d_GL>();
            //Console.WriteLine("-------------");
            for (int dir1 = -1; dir1 < 2; dir1 += 2)
            {
                for (int dir2 = -1; dir2 < 2; dir2 += 2)
                {
                    for (int dir3 = -1; dir3 < 2; dir3 += 2)
                    {
                        var _in = new double[] { d1, d2, d3, k1, k2, k3, dir1, dir2, dir3 };
                        double solv_max = 1200;
                        var r2_t = findOneVar(_in, solv_max, calc_solv, solvCond);
                        var solv = calc_solv(r2_t, _in);
                        var r1 = solv[1];
                        var r2 = r2_t;
                        var r3 = solv[2];
                        pos = trialat(P1, P2, P3, r1, r2, r3);
                        //Console.WriteLine(pos);
                        pos_all.Add(pos);
                    }
                }
            }
            return pos_all.ToArray();
        }
       
        public void calc_pos_all(PointF p1, PointF p2, PointF p3, PointF p4,
            Point3d_GL P1, Point3d_GL P2, Point3d_GL P3, Point3d_GL P4)
        {
            var positions1 = calc_pos_simple(p1, p2, p3, P1, P2, P3);
            var positions2 = calc_pos_simple(p1, p4, p3, P1, P4, P3);
            
            int ind_min = 0;
            double min_val = double.MaxValue;
            for(int i =0; i<positions1.Length;i++)
            {
                for (int j = 0; j<positions2.Length; j++)
                {
                    var del = positions1[i] - positions2[j];
                    var d_del = del.magnitude();
                    if (d_del<min_val)
                    {
                        min_val = d_del;
                        ind_min = i;
                    }
                }
            } 
            err_pos = min_val;
            pos = positions1[ind_min];
            var V1 = new Vector3d_GL(P1, pos);
            var V2 = new Vector3d_GL(P2, pos);
            var V3 = new Vector3d_GL(P3, pos);
            var V4 = new Vector3d_GL(P4, pos);
            Vector3d_GL[] v_mass = new Vector3d_GL[] { V1, V2, V3, V4 };
            PointF[] p_mass = new PointF[] { p1, p2, p3, p4 };
            calcOrient(v_mass, p_mass);
            

            oZ.normalize();
        }

        private void calcOrient(Vector3d_GL[] v_mass, PointF[] p_mass)
        {
            var sign_b = new int[10]{1, 1, 1, 1, 1,
                                    1, 1, 1, 1, 1};
            calcOrientZ(v_mass, p_mass);
            List<Vector3d_GL> OX= new List<Vector3d_GL>();
            List<Vector3d_GL> OY = new List<Vector3d_GL>();


            double eps = 0.0001;
            for (int i0 = -1; i0 < 2; i0 += 2)
            {
                sign[0] = i0;
                for (int i1 = -1; i1 < 2; i1 += 2)
                {
                    sign[1] = i1;
                    for (int i2 = -1; i2 < 2; i2 += 2)
                    {
                        sign[2] = i2;
                        for (int i3 = -1; i3 < 2; i3 += 2)
                        {
                            sign[3] = i3;
                            for (int i4 = -1; i4 < 2; i4 += 2)
                            {
                                sign[4] = i4;
                                for (int i5 = -1; i5 < 2; i5 += 2)
                                {
                                    sign[5] = i5;
                                    for (int i6 = 0; i6 < 4; i6 += 1)
                                    {
                                        sign[6] = i6;
                                        for (int i7 = 0; i7 < 4; i7 += 1)
                                        {
                                            sign[7] = i7;

                                            calcOrientXY(v_mass, p_mass);
                                            var len_xy = oX ^ oY;
                                            
                                            if ((Math.Abs(alfa_ch_x) < 0.0001) && (Math.Abs(alfa_ch_y) < 0.0001) &&
                                            Math.Abs(len_xy - Math.PI / 2) < 0.0001 &&
                                            oY.z > 0 && oX.y>0)
                                            {
                                                sign_b = sign;
                                                OX.Add(oX); OY.Add(oY);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int ind_solv = 0;
            if(OX.Count == 0)
            {

            }
            else if(OX.Count == 1)
            {
                oX = OX[ind_solv];
                oY = OY[ind_solv];
            }
            else
            {

                for (int i = 1; i < OX.Count; i++)
                {
                    if (Math.Abs(OX[i - 1].x - OX[i].x) < eps)
                    {
                        ind_solv = i;
                    }
                }

                oX = OX[ind_solv];
                oY = OY[ind_solv];
            }
            
                
            
           
            
            sign = sign_b;
        }

        private void calcOrientXY(Vector3d_GL[] v_mass, PointF[] p_mass)
        {
            oX = calc_oXy(v_mass[sign[6]], p_mass[sign[6]], PlanType.X);
            oY = calc_oXy(v_mass[sign[7]], p_mass[sign[7]], PlanType.Y);
            oX.normalize();
            oY.normalize();
        }

        
        private void calcOrientZ(Vector3d_GL[] v_mass, PointF[] p_mass)
        {
            var V1 = v_mass[0];
            var V2 = v_mass[1];
            var V3 = v_mass[2];
            var p1 = p_mass[0];
            var p2 = p_mass[1];
            var p3 = p_mass[2];
            Vector3d_GL[] ozs = new Vector3d_GL[4];
            ozs[0] = calc_oZ(V1, V2, p1, p2, 1);
            ozs[1] = calc_oZ(V2, V3, p2, p3, 1);
            ozs[2] = calc_oZ(V1, V2, p1, p2, -1);
            ozs[3] = calc_oZ(V2, V3, p2, p3, -1);
            var eps = 0.01;
            int ind_z = 0;
            var angle = 1.0;
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    angle = ozs[i] ^ ozs[3];
                }
                else
                {
                    angle = Math.Abs(ozs[i] ^ ozs[i - 1]);                   
                }
                if (angle < eps)
                {
                    ind_z = i;
                }
            }
            oZ = -ozs[ind_z];
        }

        /*private void calc_matrix(Point3d_GL[] P, PointF[] p)//A*B=C
            //1 matrix
            //Xn*A1*a1+Yn*A1*b1+1*A1*c1+yn*B1+zn*C1+1*D1 = xn'
        {
            double[,] A = new double[6, 6];
            for(int i = 0; i < )
        }*/

    }
    public class Stereovision
        {
            public Camera Camera1;
            public Camera Camera2;
            public Stereovision(Camera _camera1, Camera _camera2)
            {
                Camera1 = _camera1;
                Camera2 = _camera2;
            }
            public Point3d_GL calc_Point()
            {
                var P1 = Camera1.pos;
                var P2 = Camera2.pos;
                var P3 = Camera1.pos + Camera1.oX;
                var P4 = Camera1.pos + Camera1.oY;
                var P5 = Camera2.pos + Camera2.oX;
                var P6 = Camera2.pos + Camera2.oY;
                var P7 = Camera1.pos + Camera1.Points[0];
                var P8 = Camera2.pos + Camera2.Points[0];
                var F1 = Camera1.calc_flat(P1, P7, P4);
                var F2 = Camera1.calc_flat(P1, P7, P3);
                var F3 = Camera1.calc_flat(P2, P8, P6);
                var F4 = Camera1.calc_flat(P2, P8, P5);
                var p1 = Camera1.calc_p_3flat(F1, F2, F3);
                var p2 = Camera1.calc_p_3flat(F1, F3, F4);
                var p3 = Camera1.calc_sr_p3(p1, p2);
                return p3;
            }

        }

    public struct Flat3d_GL
    {
        public double A;
        public double B;
        public double C;
        public double D;
        public Flat3d_GL(double _A, double _B, double _C, double _D)
        {
            A = _A;
            B = _B;
            C = _C;
            D = _D;
        }
        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
        {
            var x1 = P1.x;
            var y1 = P1.y;
            var z1 = P1.z;
            var x2 = P2.x;
            var y2 = P2.y;
            var z2 = P2.z;
            var x3 = P3.x;
            var y3 = P3.y;
            var z3 = P3.z;
            A = -(y1 * z2 - y2 * z1 - y1 * z3 + y3 * z1 + y2 * z3 - y3 * z2);
            B = x1 * z2 - x2 * z1 - x1 * z3 + x3 * z1 + x2 * z3 - x3 * z2;
            C = -(x1 * y2 - x2 * y1 - x1 * y3 + x3 * y1 + x2 * y3 - x3 * y2);
            D = x1 * y2 * z3 - x1 * y3 * z2 - x2 * y1 * z3 + x2 * y3 * z1 + x3 * y1 * z2 - x3 * y2 * z1;
        }
        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Flat3d_GL F)
        {
            var x1 = P1.x;
            var y1 = P1.y;
            var z1 = P1.z;
            var x2 = P2.x;
            var y2 = P2.y;
            var z2 = P2.z;
            var A2 = F.A;
            var B2 = F.B;
            var C2 = F.C; 
            //var A2 = P1.x;
            //var B2 = P1.y;
            //var C2 = P1.z;
            A = -(B2 * z1 - C2 * y1 - B2 * z2 + C2 * y2);
            B = A2 * z1 - C2 * x1 - A2 * z2 + C2 * x2;
            C = -(A2 * y1 - B2 * x1 - A2 * y2 + B2 * x2);
            D = A2 * y1 * z2 - A2 * y2 * z1 - B2 * x1 * z2 + B2 * x2 * z1 + C2 * x1 * y2 - C2 * x2 * y1;

        }
        public double valP(Point3d_GL p)
        {
            return A * p.x + B * p.y + C * p.z + D;
        }
    }
    public struct Vector3d_GL
        {
            public double x;
            public double y;
            public double z;
            public double norm { get { return Math.Sqrt(x * x + y * y + z * z); }}
            public Vector3d_GL(double _x, double _y, double _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }
            public Vector3d_GL(Point3d_GL b, Point3d_GL a)//start,end
            {
                x = a.x - b.x;
                y = a.y - b.y;
                z = a.z - b.z;
            }
            public Vector3d_GL(Point p, double _z)
            {
                x = p.X;
                y = p.Y;
                z = _z;
            }
            public void normalize()
            {
                var n = Math.Sqrt(x * x + y * y + z * z);
                if(n!=0)
                {
                    x /= n;
                    y /= n;
                    z /= n;
                }
            }
            public static double operator *(Vector3d_GL p, Vector3d_GL p1)
            {
                return p.x * p1.x + p.y * p1.y + p.z * p1.z;
            }

            public static double operator ^(Vector3d_GL p, Vector3d_GL p1)
            {
                return Math.Acos((p * p1) / (p.norm * p1.norm));
            }

            public static Vector3d_GL operator *(Vector3d_GL p, double k)
            {
                return new Vector3d_GL(p.x * k, p.y * k, p.z * k);
            }
            public static Vector3d_GL operator *(Vector3d_GL p, float k)
            {
                return new Vector3d_GL(p.x * k, p.y * k, p.z * k);
            }
        public static Vector3d_GL operator +(Vector3d_GL p, Vector3d_GL p1)
            {
                return new Vector3d_GL(p.x + p1.x, p.y + p1.y, p.z + p1.z);
            }
            public static Vector3d_GL operator -(Vector3d_GL p, Vector3d_GL p1)
            {
                return new Vector3d_GL(p.x - p1.x, p.y - p1.y, p.z - p1.z);
            }
            public static Vector3d_GL operator -(Vector3d_GL p)
            {
                return new Vector3d_GL(-p.x , -p.y, -p.z );
            }
        public override string ToString()
        {
            return x.ToString() + " " + y.ToString() + " " + z.ToString();
        }

    }
    public class Point3d_GL
    {
        public double x;
        public double y;
        public double z;
        public Point3d_GL(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Point3d_GL(Point p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
        }
        public Point3d_GL(PointF p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
        }
        public Point3d_GL(Point3d_GL p1, Point3d_GL p2)
        {
            x = p1.x + (p2.x - p1.x) / 2;
            y = p1.y + (p2.y - p1.y) / 2;
            z = p1.z + (p2.z - p1.z) / 2;
        }
        public Point3d_GL(double[,] cor)
        {
            x = cor[0, 0];
            y = cor[1, 0];
            z = cor[2, 0];
        }
        public Point3d_GL normalize()
            {
                var norm = Math.Sqrt(x * x + y * y + z * z);
                if (norm != 0)
                {
                    x /= norm;
                    y /= norm;
                    z /= norm;
                }
                else
                {
                    x = 0;
                    y = 0;
                    z = 0;
                }

                return new Point3d_GL(x, y, z);
            }
        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double[,] ToDouble()
        {
            return new double[,] { { x }, { y }, { z }, { 1 } };
        }
        public static Point3d_GL operator *(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Point3d_GL operator *( double[,] matrixA, Point3d_GL p)
        {
            double[,] matrixB = new double[1, 1];
            if (matrixA.GetLength(0)==4)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z }, { 1 } };
            }
            else if(matrixA.GetLength(0) == 3)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z }};
            }
            else
            {
                return null;
            }

            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return null;
            }
            var matrixC = new double[matrixA.GetLength(0), matrixB.GetLength(1)];
            for (var i = 0; i < matrixA.GetLength(0); i++)
            {
                for (var j = 0; j < matrixB.GetLength(1); j++)
                {
                    matrixC[i, j] = 0;
                    for (var k = 0; k < matrixB.GetLength(0); k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            return new Point3d_GL(matrixC);
        }
       static double[,] Matrix4x4ToDouble(Matrix4x4f matrixA)
        {
            var ret = new double[4, 4];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ret[i, j] = (double)matrixA[(uint)i, (uint)j];

                }
            }
            return ret;
        }
        public static Point3d_GL operator *(Matrix4x4f matrixA, Point3d_GL p)
        {
            var matrix = Matrix4x4ToDouble(matrixA);
            return matrix*p;
        }
        public static Point3d_GL operator +(Point3d_GL p, Vector3d_GL v1)
        {
            return new Point3d_GL(p.x + v1.x, p.y + v1.y, p.z + v1.z);
        }
        public static Point3d_GL operator +(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point3d_GL operator -(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }
        public override string ToString()
        {
            
            return Math.Round(x, 4).ToString() + " "+ Math.Round(y, 4).ToString() + " "+ Math.Round(z, 4).ToString();
        }
    }
    public class Frame
    {
        public Mat im;
        public Mat im_sec;
        public Point3d_GL pos_cam;
        public Point3d_GL pos_rob;
        public Point3d_GL pos_rob_or;
        public string name;
        public Size size;
        public PointF[] points;
        public FrameType type;
        public Camera camera;
        public double size_mark;
        public DateTime dateTime;
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name, PointF[] _points)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            points = _points;
            size = _im.Size;
            type = FrameType.Las;
        }
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name, PointF[] _points, Point3d_GL _pos_rob_or)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            points = _points;
            size = _im.Size;
            pos_rob_or = _pos_rob_or;
            type = FrameType.Pos;
        }
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            size = _im.Size;
            type = FrameType.Las;

        }
        public Frame(Mat _im, string _name)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            type = FrameType.Test;

        }

        public Frame(Mat _im, string _name, FrameType frameType)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            type = frameType;

        }
        public Frame(Mat _im, Mat _im2, string _name)
        {
            im = _im;
            im_sec = _im2;
            name = _name;
            size = _im.Size;
            type = FrameType.Stereo ;

        }
        override public string ToString()
        {
            return name;
        }
    }
    public class VideoFrame
    {
        public Mat[] im;
        public Point3d_GL pos_rob;
        public string name;
        public VideoFrame(Mat[] _im, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_rob = _pos_rob;
            name = _name;
        }
        override public string ToString()
        {
            return name;
        }

        public Frame toFrame()
        {
            return new Frame(findMostWhite(im),new Point3d_GL(0,0,0),pos_rob,name,new PointF[1]);
        }
        Mat findMostWhite(Mat[] mats)
        {
            var sort_mats = from mat in mats
                            orderby calcWhiteIm(mat) descending
                            select mat;
            return sort_mats.ToArray()[0];
        }
        int calcWhiteIm(Mat mat)
        {
            int ret = 0;
            var im = mat.ToImage<Gray, Byte>();
            for (int x = 0; x < mat.Width; x++)
            {
                for (int y = 0; y < mat.Height; y++)
                {
                    if (im.Data[y, x, 0] > 252)
                    {
                        ret++;
                    }
                }
            }
            return ret;
        }
    }
    public struct Line3d_GL
    {
        public Point3d_GL k;
        public Point3d_GL p;
        public Line3d_GL(Vector3d_GL _k, Point3d_GL _p)
        {
            k = new Point3d_GL(_k.x,_k.y,_k.z);
            p = _p;
        }
        public Line3d_GL(Point3d_GL _k, Point3d_GL _p)
        {
            k = _k;
            p = _p;
        }
        public Point3d_GL calcCrossFlat(Flat3d_GL F)
        {
            var del = F.A * k.x + F.B * k.y + F.C * k.z;
            var num = F.A * p.x + F.B * p.y + F.C * p.z + F.D;
            // Console.WriteLine(num + " " + del);
           // Console.WriteLine(p+" "+k);
            if (del!=0)
            {
                var t = -num / del;
                return new Point3d_GL(
                    k.x * t + p.x,
                     k.y * t + p.y,
                     k.z * t + p.z);
            }
            else
            {
                return new Point3d_GL(0,0,0);
            }
        }
    }
    
    public struct Flat_4P
    {
        public Flat3d_GL[] F;
        public Point3d_GL[] P;

        public Flat_4P(Point3d_GL[] _p)
            // 0 *----* 1
            //   | \  |
            //   |  \ |
            // 2 *----* 3  
        {
            F = new Flat3d_GL[8];
            F[0] = new Flat3d_GL(_p[0], _p[2], _p[3]);
            F[1] = new Flat3d_GL(_p[2], _p[0], F[0]);
            F[2] = new Flat3d_GL(_p[3], _p[2], F[0]);
            F[3] = new Flat3d_GL(_p[0], _p[3], F[0]);

            F[4] = new Flat3d_GL(_p[0], _p[3], _p[1]);
            F[5] = new Flat3d_GL(_p[3], _p[0], F[4]);
            F[6] = new Flat3d_GL(_p[1], _p[3], F[4]);
            F[7] = new Flat3d_GL(_p[0], _p[1], F[4]);
            var ps = new List<Point3d_GL>();
            foreach(var P in _p)
            {
                ps.Add(P);
            }
            P = ps.ToArray();
            verifigateSignature();
        }
        private void verifigateSignature()
        {

            if (F[1].valP(P[3])<0)
            {
                var tempF = new Flat3d_GL(-F[1].A, -F[1].B, -F[1].C, F[1].D);
                F[1] = tempF;
                
            }

            if (F[2].valP(P[0]) < 0)
            {
                var tempF = new Flat3d_GL(-F[2].A, -F[2].B, -F[2].C, F[2].D);
                F[2] = tempF;
            }

            if (F[3].valP(P[2]) < 0)
            {
                var tempF = new Flat3d_GL(-F[3].A, -F[3].B, -F[3].C, F[3].D);
                F[3] = tempF;
            }
            //--------------------------------
            if (F[5].valP(P[1]) < 0)
            {
                var tempF = new Flat3d_GL(-F[5].A, -F[5].B, -F[5].C, F[5].D);
                F[5] = tempF;
            }

            if (F[6].valP(P[0]) < 0)
            {
                var tempF = new Flat3d_GL(-F[6].A, -F[6].B, -F[6].C, F[6].D);
                F[6] = tempF;
            }

            if (F[7].valP(P[3]) < 0)
            {
                var tempF = new Flat3d_GL(-F[7].A, -F[7].B, -F[7].C, F[7].D);
                F[7] = tempF;
            }
        }
        bool checkInside(Point3d_GL p,int ind)
        {
            bool ret = true;
            for(int i=0; i<3;i++)
            {
                if (F[ind+i].valP(p) < -20)
                {
                    return false;
                }
            }
            return ret;
        }
        public Point3d_GL crossLine(Line3d_GL line)
        {
            var pCposs = line.calcCrossFlat(F[0]);
            
            if(checkInside(pCposs, 1))
            {
                return pCposs;
            }
            pCposs = line.calcCrossFlat(F[4]);
            if (checkInside(pCposs, 5))
            {
                return pCposs;
            }
            return null;
        }
    }

}