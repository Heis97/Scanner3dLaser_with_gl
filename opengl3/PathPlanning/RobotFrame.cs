using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using PathPlanning;

namespace opengl3
{

    public struct PositionRob
    {
        public Point3d_GL position;
        public Point3d_GL rotation;
        public PositionRob(Point3d_GL pos, Point3d_GL rot)
        {
            this.position = pos;
            this.rotation = rot;
        }
    }

    public class RobotFrame
    {
        public double X , Y, Z, A, B, C, V, D,F;//v - rob vel, f - disp vel
        public double[] q;
        public PositionRob frame;
        public Color3d_GL color;
        public int[] turn;
        public enum RobotType { KUKA = 1, PULSE = 2, FABION2 = 3};

        public RobotType robotType;


        public RobotFrame(double x = 0, double y = 0, double z = 0, double a = 0, double b = 0, double c = 0, double v = 0, double d = 0, double f = 0, RobotType robotType = RobotType.PULSE)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            V = v;
            D = d;
            F = f;
            frame = new PositionRob(new Point3d_GL(x, y, z), new Point3d_GL(a, b, c));
            this.robotType = robotType;
        }

        public RobotFrame(string coords, RobotType robotType = RobotType.PULSE)
        {
            var coords_s = coords.Split(' ');
            if (coords_s.Length < 6)
                return;
            X = Convert.ToDouble(coords_s[0]);
            Y = Convert.ToDouble(coords_s[1]);
            Z = Convert.ToDouble(coords_s[2]);
            A = Convert.ToDouble(coords_s[3]);
            B = Convert.ToDouble(coords_s[4]);
            C = Convert.ToDouble(coords_s[5]);
            V = 0;
            D = 0;
            this.robotType = robotType;
        }
        public Point3d_GL get_pos() { return new Point3d_GL(X, Y, Z); }
        public Point3d_GL get_rot() { return new Point3d_GL(A, B, C); }

        public RobotFrame set_pos(Point3d_GL p) 
        {
            frame.position = p;
            X = p.x;
            Y = p.y;
            Z = p.z;
            return this;
        }

        public RobotFrame set_rot(Point3d_GL r)
        {
            frame.position = r;
            A = r.x;
            B = r.y;
            C = r.z;
            return this;
        }
        public Matrix<double>  getMatrix(RobotType robotType = RobotType.PULSE)
        {

            return ABCmatr(X, Y, Z, A, B, C, robotType);
        }
        static public Matrix<double> getMatrixPos (PositionRob pos, RobotType robotType = RobotType.PULSE)
        {

            return ABCmatr(pos.position.x, pos.position.y, pos.position.z, pos.rotation.x, pos.rotation.y, pos.rotation.z, robotType);
        }
        public RobotFrame(Matrix<double> m, RobotType type = RobotType.KUKA)
        {
            robotType = type;
            switch (type)
            {
                case RobotType.KUKA:
                    {
                        var x = m[0, 3];
                        var y = m[1, 3];
                        var z = m[2, 3];

                        var sRy = m[0, 2];
                        var cRy = Math.Pow((1-sRy*sRy), 0.5);

                        var sRz = -m[0, 1] / cRy;
                        var cRz = m[0, 0] / cRy;

                        var sRx = -m[1, 2] / cRy;
                        var cRx = m[2, 2] / cRy;

                        //Console.WriteLine(cRx + " "+ sRx+" "+arccos(cRx));
                        var Rx = Math.Sign(sRx) * arccos(cRx);
                        var Ry = Math.Asin(m[0, 2]);
                        var Rz = Math.Sign(sRz) * arccos(cRz);

                        int d = (int)m[3, 3];
                        X = x;
                        Y = y;
                        Z = z;
                        A = Rz;
                        B = -Ry;
                        C = Rx;
                        D = d;
                    }
                    break;
                case RobotType.PULSE:
                    {
                        var x = m[0,3];
                        var y = m[1,3];
                        var z = m[2,3];

                        var sRy = m[0,2];
                        var cRy = Math.Pow((1 - sRy*sRy),0.5);

                        var sRz = -m[0,1] / cRy;
                        var cRz = m[0,0] / cRy;

                        var sRx = -m[1, 2] / cRy;
                        var cRx = m[2, 2] / cRy;

                        //Console.WriteLine(cRx + " "+ sRx+" "+arccos(cRx));
                        var Rx = Math.Sign(sRx) * arccos(cRx);
                        var Ry = Math.Asin(m[0, 2]);
                        var Rz = Math.Sign(sRz) * arccos(cRz);

                        int d = (int)m[3, 3];
                        X = x;
                        Y = y;
                        Z = z;
                        A = -Rx;
                        B = Ry;
                        C = Rz;
                        D = d;
                    }
                    break;
                case RobotType.FABION2:
                    {
                        var x = m[0, 3];
                        var y = m[1, 3];
                        var z = m[2, 3];


                        X = x;
                        Y = y;
                        Z = z;
                        A = 0;
                        B = 0;
                        C = 0;
                        D = 3;
                    }
                    color = new Color3d_GL((float)m[3, 0], (float)m[3, 1], (float)m[3, 2]);
                    break;
            }
            frame = new PositionRob(new Point3d_GL(X, Y, Z), new Point3d_GL(A, B, C));
        }



        static public double dist(RobotFrame fr1, RobotFrame fr2)
        {
            return Math.Sqrt((fr1.X - fr2.X) * (fr1.X - fr2.X) +
                (fr1.Y - fr2.Y) * (fr1.Y - fr2.Y) +
                (fr1.Z - fr2.Z) * (fr1.Z - fr2.Z));
        }

        static public string generate_string(RobotFrame[] frames)
        {
            
            var traj_rob = new StringBuilder();


            for (int i = 0; i < frames.Length; i++)
            {
                traj_rob.Append("G1"+frames[i].ToStr());
            }
            return traj_rob.ToString();
        }
        static public string generate_string_fabion(RobotFrame[] frames)
        {

            var traj_rob = new StringBuilder();

            var num = 1;

            for (int i = 0; i < frames.Length; i++)
            {
                if(i==0)
                {
                    traj_rob.Append(frames[i].ToStr_start(num));
                    num += 3;
                }
                else if(i==frames.Length-1)
                {
                    traj_rob.Append(frames[i].ToStr_stop(num));
                }
                else
                {

                    traj_rob.Append(frames[i].ToStr_prog(num));
                    num++;

                }
                
            }
            return traj_rob.ToString();
        }

        static public RobotFrame operator +(RobotFrame fr1, RobotFrame fr2)
        {
            var fr = fr1.Clone();
            fr.X += fr2.X; fr.Y += fr2.Y; fr.Z += fr2.Z;
            fr.A += fr2.A; fr.B += fr2.B; fr.C += fr2.C;
            return fr;
        }
        static public RobotFrame operator -(RobotFrame fr1, RobotFrame fr2)
        {
            var fr = fr1.Clone();
            fr.X -= fr2.X; fr.Y -= fr2.Y; fr.Z -= fr2.Z;
            fr.A -= fr2.A; fr.B -= fr2.B; fr.C -= fr2.C;
            return fr;
        }
        static public RobotFrame operator *(RobotFrame fr1, double k)
        {
            var fr = fr1.Clone();
            fr.X *= k; fr.Y *= k; fr.Z *= k;
            fr.A *= k; fr.B *= k; fr.C *= k;
            return fr;
        }
        static public RobotFrame operator /(RobotFrame fr1, double k)
        {
            var fr = fr1.Clone();
            fr.X /= k; fr.Y /= k; fr.Z /= k;
            fr.A /= k; fr.B /= k; fr.C /= k;
            return fr;
        }
        public override string ToString()
        {
            return " X" + round(X) + ", Y" + round(Y) + ", Z" + round(Z) +
                    ", A" + round(A) + ", B" + round(B) + ", C" + round(C) +
                    ", V" + round(V) + ", D" + D + " \n";
        }

        public string ToStr(string del = " ",bool nums = false)
        {
            var str = del + "X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) +
                    del + "A" + round(A) + del + "B" + round(B) + del + "C" + round(C) +
                    del + "F" + round(F) + del + "V" + round(V) + del + "D" + D + " \n";
            if (nums)
            {
                str = del +  round(X) + del +  round(Y) + del + round(Z) +
                    del +  round(A) + del +  round(B) + del +  round(C) +
                    del +  round(F) + del +  round(V) + del +  D;
            }
            return str;
        }
        public string ToStr_start(int num, string del = " ")
        {
            var str = "N"+(num*5)+ del + " G11 X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z+10) +  " D3 \n"; num++;
            str += "N" + (num * 5) + del + " G11 X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) +  " D3 \n"; num++;
            str += "N" + (num * 5) + " G87 P0 P3 P0.1 \n"; 
            return str;
        }
        public string ToStr_stop(int num, string del = " ")
        {
            var str = "N" + (num * 5) + del + " G11 X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z + 10) +  " D3 \n"; 
            return str;
        }
        public string ToStr_prog(int num, string del = " ")
        {
            if (color.r > 0)
            {
                var str = "N" + (num * 5) + del + " G88 X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) + del + "F" + round(F) + del + "V" + round(V) + " D3 Q0 T1 I0 J0\n";

                return str;
            }
            else
            {
                var str = "N" + (num * 5) + del + " G88 X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) + del + "F" + round(F) + del + "V0 D3 Q0 T1 I0 J0\n";
                return str;
            }

            
        }
        static double round(double val)
        {
            return Math.Round(val, 4);
        }

        public RobotFrame Clone()
        {
            return new RobotFrame(X, Y, Z, A, B, C, V, D,F,robotType);
        }
        //---------------------------------------------------------------------------------------
        static public List<RobotFrame> divide_line(RobotFrame frame1, RobotFrame frame2, double d)
        {
            var divide_frames = new List<RobotFrame>();
            var delt_fr = frame2 - frame1;
            var dist_fr = dist(frame1, frame2);
            var count = (int)(dist_fr / d);
            divide_frames.Add(frame1);
            for (int i = 1; i < count; i++)
            {
                var fr = frame1 + delt_fr * ((double)i / count);
                fr.V = frame2.V; fr.F = frame2.F; fr.D = frame2.D;
                divide_frames.Add(fr);
            }
            divide_frames.Add(frame2);
            return divide_frames;
        }
        static public RobotFrame[] divide_line(RobotFrame[] frames, double d)
        {
            var divide_frames = new List<RobotFrame>();
            for(int i=1; i< frames.Length; i++)
            {
                var frms = divide_line(frames[i-1], frames[i],d);
                divide_frames.AddRange(frms);
            }
            return divide_frames.ToArray();
        }
        static public List<RobotFrame> smooth_frames_gauss(List<RobotFrame> frames, int w)
        {
            var smooth_frames = new List<RobotFrame>();
            for (int i = 0; i < frames.Count; i++)
            {
                int start_i = i - w;
                int count = 2 * w;
                if (start_i < 0) start_i = 0;
                if (start_i + count > frames.Count) count = frames.Count - 1 - start_i;
                var wind = frames.GetRange(start_i, count);
                smooth_frames.Add(get_average_frame(wind));
            }

            return smooth_frames;
        }

        static public List<RobotFrame> smooth_frames_radius(List<RobotFrame> frames, double r,double min_dist)
        {         
            return PathPlanner.gen_smooth_arc(frames,r,min_dist);
        }
        static public  List<RobotFrame> smooth_angle(List<RobotFrame> frames, int w)
        {
            var smooth_frames = new List<RobotFrame>();
            for (int i = 0; i < frames.Count; i++)
            {
                int start_i = i - w;
                int count =  2 * w;
                if(start_i < 0) start_i = 0;
                if(start_i + count > frames.Count) count = frames.Count - 1 - start_i;
                var wind = frames.GetRange(start_i, count);
                smooth_frames.Add(get_average_angle(wind, frames[i].Clone()));
            }
            return smooth_frames;
        }
        static public RobotFrame[] comp_acs(List<RobotFrame> frames, double dt)
        {
            Console.WriteLine("COMP ACS: ");
            var smooth_frames = new List<RobotFrame>();
            for (int i = 2; i < frames.Count; i++)
            {
                var d1 = frames[i] - frames[i-1];
                var d2 = frames[i-1] - frames[i - 2];
                var d = d1 - d2;
                var a = d/(dt*dt);

                var p1 = frames[i - 2].get_pos();
                var p2 = frames[i - 1].get_pos();
                var p3 = frames[i].get_pos();
                var circ  = Camera.calcCirc(p1, p2, p3);
                smooth_frames.Add(a);
                Console.WriteLine(circ[2] + d1.ToStr(" ", true));// + " "+ a.ToStr(" ", true));
            }
            Console.WriteLine("/END COMP ACS ");
            return smooth_frames.ToArray();
        }


        static public List<RobotFrame> decrease_angle(List<RobotFrame> frames, double k_decr)//1 full ang,0 const ang
        {
            var aver_fr = new RobotFrame(0,0,0,0,0,0,0,0,0,RobotType.PULSE);
            aver_fr = get_average_angle(frames, aver_fr);
            var frames_decrease = new List<RobotFrame>();
            for (int i = 0; i < frames.Count; i++)
            {
                var fr = frames[i].Clone();
                fr.A = aver_fr.A + k_decr * (frames[i].A - aver_fr.A);
                fr.B = aver_fr.B + k_decr * (frames[i].B - aver_fr.B);
                fr.C = aver_fr.C + k_decr * (frames[i].C - aver_fr.C);

                frames_decrease.Add(fr);
            }
            return frames_decrease;
        }

        static RobotFrame get_average_angle(List<RobotFrame> frames, RobotFrame target_frame )
        {
            double a = 0, b=0, c = 0;
            for(int i=0; i<frames.Count;i++)
            {
                a += frames[i].A;
                b += frames[i].B;
                c += frames[i].C;
            }
            target_frame.A = a / frames.Count;
            target_frame.B = b / frames.Count;
            target_frame.C = c / frames.Count;
            return target_frame;
        }
        static RobotFrame get_average_frame(List<RobotFrame> frames)
        {
            var fr_last = frames[frames.Count-1];
            var fr_av = new RobotFrame();
            for (int i = 0; i < frames.Count; i++)
            {
                fr_av += frames[i];
            }
            fr_av /= frames.Count;
            fr_av.F = fr_last.F;
            fr_av.V = fr_last.V;
            fr_av.D = fr_last.D;
            return fr_av;
        }


        static public Matrix<double> RotZmatr(double alpha)
        {
            var matrix = new Matrix<double>(new double[,] {
                { cos(alpha), -sin(alpha), 0,0 },
                { sin(alpha), cos(alpha), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 } });
            return matrix;
        }
        static public Matrix<double> RotYmatr(double alpha)
        {
            var matrix = new Matrix<double>(new double[,] {
                 { cos(alpha),0, sin(alpha), 0 },
                { 0,1,0 , 0 },
                {  -sin(alpha), 0, cos(alpha), 0 },
                 { 0, 0, 0, 1 }});
            return matrix;
        }
        static public Matrix<double> RotXmatr(double alpha)
        {
            var matrix = new Matrix<double>(new double[,] {
                { 1,0,0,0 },
                { 0, cos(alpha), -sin(alpha), 0 },
                { 0, sin(alpha), cos(alpha), 0 },
                 { 0, 0, 0, 1 } });
            return matrix;
        }
        static public Matrix<double> Translmatr(double x, double y, double z)
        {
            var matrix = new Matrix<double>(new double[,] {
                { 1, 0, 0, x },
                { 0, 1, 0, y },
                { 0, 0, 1, z },
                { 0, 0, 0, 1 } });
            return matrix;
        }
        static public Matrix<double> ABCmatr(double X, double Y, double Z, double A, double B, double C,RobotType robotType)
        {
            if (robotType == RobotType.KUKA)
            {
                var matrix = Translmatr(X, Y, Z) * RotZmatr(A) * RotYmatr(B) * RotXmatr(C);
                return matrix;
            }
            else 
            {
                var matrix =  RotXmatr(A) * RotYmatr(B) * RotZmatr(C);
                matrix[0, 3] = X;
                matrix[1, 3] = Y;
                matrix[2, 3] = Z;
                return matrix;
            }
        }

        static public double cos(double alpha)
        {
            return Math.Cos(alpha);
        }
        static public double sin(double alpha)
        {
            return Math.Sin(alpha);
        }
        public static double arccos(double cos)
        {
            double _cos = cos;
            if (_cos >= 1) _cos = 1;
            if (_cos <= -1) _cos = -1;
            return Math.Acos(_cos);

        }
        static public Matrix<double> create_dhmatr(double[] vals)
        {
            var theta = vals[0];
            var alpha = vals[1];
            var a = vals[2];
            var d = vals[3];

            double cosQ = Math.Cos(theta);
            double sinQ = Math.Sin(theta);
            double cosA = Math.Cos(alpha);
            double sinA = Math.Sin(alpha);
            var matr = new double[,]
            {{ cosQ,-sinQ*cosA,sinQ*sinA,a*cosQ },
            { sinQ, cosQ*cosA,-cosQ*sinA,a*sinQ },
            { 0   , sinA     , cosA    ,  d },
            { 0   , 0        , 0       ,  1 } };
            return new Matrix<double>(matr);
        }
        static Matrix<double> eye_matr(int w)
        {
            var matr = new double[w, w];
            for(int i = 0; i < w; i++)
                for(int j = 0; j < w; j++)
                {
                    if (i == j) matr[i, j] = 1;
                    else matr[i, j] = 0;
                }
            return new Matrix<double>(matr);
        }
        static public Matrix<double> calc_pos(double[][] vals)
        {
            var matrs = new List<Matrix<double>>();
            foreach(var val in vals)            
                matrs.Add(create_dhmatr(val));
            
            var matr_res = eye_matr(4);
            foreach(var matr in matrs)
                matr_res *= matr;

            return matr_res;
        }
        static double[] to_rad(double[] q)
        {
            var q_r = new double[q.Length];
            for(int i =0; i<q.Length;i++)
            {
                q_r[i] = q[i] * Math.PI / 180;
            }
            return q_r;
        }
        static public PositionRob comp_forv_kinem(double[] q,bool rad = true, RobotType robotType = RobotType.PULSE)
        {
            if(robotType==RobotType.PULSE)
            {
                var L1 = 0.2311;
                var L2 = 0.45;
                var L3 = 0.37;
                var L4 = 0.1351;
                var L5 = 0.1825;
                var L6 = 0.1325;

                if(!rad)
                    q = to_rad(q);

                var dh_params = new double[][] {
                    new double[]{ q[0], Math.PI / 2, 0, L1},
                    new double[]{ q[1],  0, -L2, 0},
                    new double[]{ q[2],  0, -L3, 0},
                    new double[]{ q[3], Math.PI / 2, 0, L4},
                    new double[]{ q[4], -Math.PI / 2, 0, L5},
                    new double[]{ q[5],  0, 0, L6}
                };

                var fr = new RobotFrame(calc_pos(dh_params), robotType);
                var pos = fr.frame;
                return pos;
            }

            return new PositionRob();
        }

        public double[][] comp_inv_kinem(PositionRob pos)
        {
            var solvs = new List<double[]>();
            var turns = new int[][]
            {
               new int[] { -1, -1, -1 },
                new int[]{-1, -1, 1 },
                new int[]{-1, 1, -1 },
                new int[]{-1, 1, 1},
                new int[]{1, -1, -1},
                new int[]{1, -1, 1},
                new int[]{1, 1, -1},
                new int[]{1, 1, 1}
            };
            foreach (var turn in turns)
                solvs.Add(comp_inv_kinem_priv(pos, turn));

            return solvs.ToArray();
        }
        public static Point3d_GL calc_inters_2circ(double x1, double y1, double x2, double y2, double R1, double R2, double sign)
        {
            var p1 = new Point3d_GL(x1, y1);
            var p2 = new Point3d_GL(x2, y2);
            if ((p2 - p1).magnitude() > R1 + R2) return Point3d_GL.notExistP();
            x2 -= x1;
            y2 -= y1;
            double sD = sign * Math.Sqrt((R1 * R1 + 2 * R1 * R2 + R2 * R2 - x2 * x2 - y2 * y2) * (2 * R1 * R2 - R1 * R1 - R2 * R2 + x2 * x2 + y2 * y2));
            double x = (R1 * R1 - R2 * R2 + x2 * x2 + y2 * y2 - ((y2 * (R1 * R1 * y2 - R2 * R2 * y2 + x2 * x2 * y2 + y2 * y2 * y2 + x2 * sD)) / (x2 * x2 + y2 * y2))) / (2 * x2);
            double y = (R1 * R1 * y2 - R2 * R2 * y2 + x2 * x2 * y2 + y2 * y2 * y2 + x2 * sD) / (2 * (x2 * x2 + y2 * y2));
            x += x1;
            y += y1;
            return new Point3d_GL(x, y);
        }
        static public double[] comp_inv_kinem_priv(PositionRob pos, int[] turn)
        {
            var pm = getMatrixPos(pos);
            var p = new Point3d_GL(pm[0, 3], pm[1, 3], pm[2, 3]);
            var L1 = 0.2311;
            var L2 = 0.45;
            var L3 = 0.37;
            var L4 = 0.1351;
            var L5 = 0.1825;
            var L6 = 0.1325;

            var dz = eye_matr(4);
            dz[2, 3] = -L6;
            var p0 = pm * dz;
            var p0p = new Point3d_GL(p0[0, 3], p0[1, 3], p0[2, 3]);
            var vz = new Point3d_GL(pm[0, 2], pm[1, 2], pm[2, 2]);
            var xy_d = p0p.magnitude_xy();
            if (Math.Abs(L4) > Math.Abs(xy_d)) return null;
            var aa_d = Math.Sqrt(xy_d * xy_d - L4 * L4);
            var p_circ_cross = calc_inters_2circ(0, 0, p0p.x, p0p.y, aa_d, L4, turn[0]);
            if (!p.exist) return null;
            var x2 = p_circ_cross.x;
            var y2 = p_circ_cross.y;
            var vf = new Point3d_GL(x2 - p0p.x, y2 - p0p.y);
            vf = vf.normalize();
            var ax5y = Point3d_GL.vec_perpend_2_vecs(vz, vf);
            ax5y *= turn[1];

            var p1 = p0p + ax5y * L5;
            var p2 = p1 + vf * L4;

            var scara = p2 - new Point3d_GL(0, 0, L1);
            var sq1 = -scara.y / scara.magnitude_xy();
            var cq1 = -scara.x / scara.magnitude_xy();

            var q1 = Math.Sign(sq1) * arccos(cq1);


            var Ls = scara.magnitude_xy();
            var Lt = scara.magnitude();

            if (Ls == 0) return null;
            if (Lt == 0) return null;
            var omega = Math.Atan(scara.z / Ls);
            var theta = arccos((L2 * L2 + L3 * L3 - Lt * Lt) / (2 * L2 * L3));
            var omega_ext = arccos((L2 * L2 + Lt * Lt - L3 * L3) / (2 * L2 * Lt));
            omega += omega_ext * turn[2];

            var q2 = -omega;
            var q3 = Math.PI - theta;

            if (turn[0] > 0) {
                q1 += Math.PI;


                q2 *= -1;
                q2 -= Math.PI;
                q3 *= -1;
            }

            if (turn[2] < 0) {
                q3 *= turn[2];
            }
            var ax4z = -ax5y;
            var ax4y = -vf;
            var ax4x = ax4y | ax4z;

            var dh_params = new double[][]
                {
                    new double[]{ q1, Math.PI / 2, 0, L1 },
                    new double[]{ q2,  0, -L2, 0 },
                    new double[]{ q3,  0, -L3, 0 } };

            var pm3 = calc_pos(dh_params);
            var ax3x = new Point3d_GL(pm3[0,0], pm3[1,0], pm3[2,0]);
            var s4 = Point3d_GL.sign_r_v(ax3x, ax4x, ax4y);
            var q4 = s4 * Point3d_GL.ang(ax3x ,ax4x); 


            var ax6y = new Point3d_GL(pm[0,1], pm[1,1], pm[2,1]);
            var ax6z = new Point3d_GL(pm[0,2], pm[1,2], pm[2,2]);

            var s6 = Point3d_GL.sign_r_v(ax5y, ax6y, ax6z);
            var q6 = s6 *  Point3d_GL.ang(ax5y ,ax6y);



            var s5 = Point3d_GL.sign_r_v(ax4y, ax6z, ax4z);
            var q5 = s5 *  Point3d_GL.ang(ax4y ,ax6z);

            q6 += Math.PI;

            var q = new double[]{q1, q2, q3, q4, q5, q6};
            for (int i=0 ;i<q.Length;i++){
                var qi = q[i];
                if (qi > Math.PI) qi -= 2 * Math.PI;
                if (qi < -Math.PI) qi += 2 * Math.PI;
                q[i] = qi;
            }

            return q;
        }


        //---------------------------------------------------------------------
        static public Matrix<double> mult_frame(Matrix<double> rob_base, Matrix<double> frame)
        {
            var transl = rob_base * frame;
            var frame_tr = frame.Transpose();
            frame_tr[3, 0] = 0;
            frame_tr[3, 1] = 0;
            frame_tr[3, 2] = 0;
            //prin.t(frame_tr);
            var rot = rob_base * frame_tr;

            rot[0, 3] = transl[0, 3];
            rot[1, 3] = transl[1, 3];
            rot[2, 3] = transl[2, 3];
            
            return rot;
        }

        static public Matrix<double> mult_frame_2(Matrix<double> rob_base, Matrix<double> frame)
        {
            var transl = rob_base * frame;
           /* var frame_tr = frame.Transpose();
            frame_tr[3, 0] = 0;
            frame_tr[3, 1] = 0;
            frame_tr[3, 2] = 0;*/
            //prin.t(frame_tr);
            var rot = frame * rob_base ;

            rot[0, 3] = transl[0, 3];
            rot[1, 3] = transl[1, 3];
            rot[2, 3] = transl[2, 3];

            return rot;
        }

        static public Matrix<double> toVcs(Matrix<double> matr)
        {
            var vcs = matr.Transpose();
            vcs[0, 3] = vcs[3, 0];
            vcs[1, 3] = vcs[3, 1];
            vcs[2, 3] = vcs[3, 2];
            vcs[3, 0] = 0;
            vcs[3, 1] = 0;
            vcs[3, 2] = 0;
            return vcs;
        }



    }
}
