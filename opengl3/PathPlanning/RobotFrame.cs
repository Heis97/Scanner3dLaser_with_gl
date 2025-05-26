using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Stitching;
using PathPlanning;

namespace opengl3
{

    public struct PositionRob
    {
        public Point3d_GL position;
        public Point3d_GL rotation;
        public Matrix<double> m;
        //public RobotFrame.RobotType robot_type;
        public PositionRob(Point3d_GL pos, Point3d_GL rot, Matrix<double> m = null)
        {
            this.position = pos;
            this.rotation = rot;
            this.m = m;
        }

        public override string ToString()
        {
            return position.ToString()+" "+rotation.ToString();
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
        public DateTime timestamp;

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

        public RobotFrame(PositionRob pos, double v = 0, double d = 0, double f = 0, RobotType robotType = RobotType.PULSE)
        {
            X = pos.position.x;
            Y = pos.position.y;
            Z = pos.position.z;
            A = pos.rotation.x;
            B = pos.rotation.y;
            C = pos.rotation.z;
            V = v;
            D = d;
            F = f;
            frame = pos;
            this.robotType = robotType;
        }

        public RobotFrame(Pose pose, RobotType robotType)
        {
            var pos = comp_forv_kinem(pose.angles.ToArray(), pose.angles.Count, pose.radians, robotType);
            X = pos.position.x;
            Y = pos.position.y;
            Z = pos.position.z;
            A = pos.rotation.x;
            B = pos.rotation.y;
            C = pos.rotation.z;
            frame = pos;
            timestamp = pose.timestamp;
            this.robotType = robotType;
        }
        public double str_to_double(string s)
        {
            
            var s1 = s;
            if (s1.Contains(","))
            {
                s1.Replace(',', '.');
            }
            if (!s1.Contains("."))
            {
                s1 += ".0";
            }
            return Convert.ToDouble(s1);
        }
        public RobotFrame(string coords, RobotType robotType = RobotType.KUKA, bool rad = true)
        {
            var coords_w = coords.ToLower();
            var coords_s = coords_w.Trim().Split(' ');
            if (coords_s.Length < 6)
                return;
            if(coords_w.Contains('x'))
            {
                X = str_to_double(coords_s[1].Substring(1));
                Y = str_to_double(coords_s[2].Substring(1));
                Z = str_to_double(coords_s[3].Substring(1));
                A = str_to_double(coords_s[4].Substring(1));
                B = str_to_double(coords_s[5].Substring(1));
                C = str_to_double(coords_s[6].Substring(1));
            }
            else
            {
                X = str_to_double(coords_s[0]);
                Y = str_to_double(coords_s[1]);
                Z = str_to_double(coords_s[2]);
                A = str_to_double(coords_s[3]);
                B = str_to_double(coords_s[4]);
                C = str_to_double(coords_s[5]);
            }
            if(!rad)
            {
                A = to_rad(A);
                B = to_rad(B);
                C = to_rad(C);
            }
            this.robotType = robotType;
            if (coords_w.Contains("k"))
            {
                this.robotType = RobotType.KUKA;
            }
            /*if (coords_w.Contains("p"))
            {
                this.robotType = RobotType.PULSE;
            }*/
            this.frame = new PositionRob(new Point3d_GL(X, Y,Z), new Point3d_GL(A, B,C));
            V = 0;
            D = 0;
            //Console.WriteLine(this.robotType);
        }

        public static RobotFrame[] parse_g_code(string g_code, RobotType robotType = RobotType.PULSE)
        {
            var lines = g_code.Split('\n');
            var frames = new List<RobotFrame>();
            for(int i=0; i < lines.Length; i++)
            {
                if(lines[i].Length>12)
                {
                    frames.Add(new RobotFrame(lines[i],robotType));
                }
            }
            return frames.ToArray();
        }
        public Point3d_GL get_pos() { return new Point3d_GL(X, Y, Z); }
        public Point3d_GL get_rot() { return new Point3d_GL(A, B, C); }
        public PositionRob get_position_rob() { return new PositionRob(new Point3d_GL(X, Y, Z),new Point3d_GL(A, B, C)); }
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
        public Matrix<double>  getMatrix()
        {

            return ABCmatr(X, Y, Z, A, B, C, robotType);
        }
        static public Matrix<double> getMatrixPos (PositionRob pos, RobotType robotType = RobotType.PULSE)
        {

            return ABCmatr(pos.position.x, pos.position.y, pos.position.z, pos.rotation.x, pos.rotation.y, pos.rotation.z, robotType);
        }
        public RobotFrame(Matrix<double> m, RobotType type = RobotType.PULSE)
        {
            robotType = type;
            //Console.WriteLine(robotType.ToString());
            switch (type)
            {
                case RobotType.KUKA:
                    {
                        var x = m[0, 3];
                        var y = m[1, 3];
                        var z = m[2, 3];

                        var sRy = -m[2, 0];

                        var cRy = Math.Pow((1-sRy*sRy), 0.5);

                        var sRz = m[1,0] / cRy;
                        var cRz = m[0, 0] / cRy;

                        var sRx = m[2, 1] / cRy;
                        var cRx = m[2, 2] / cRy;

                        //Console.WriteLine(cRx + " "+ sRx+" "+arccos(cRx));
                        var Rx = Math.Sign(sRx) * arccos(cRx);
                        var Ry = Math.Asin(sRy);
                        var Rz = Math.Sign(sRz) * arccos(cRz);

                        int d = (int)m[3, 3];
                        X = x;
                        Y = y;
                        Z = z;
                        A = cut_off_2pi(Rz);
                        B = cut_off_2pi(Ry);
                        C = cut_off_2pi(Rx);
                        D = d;
                    }
                    break;
                case RobotType.PULSE:
                    {
                        var x = m[0,3];
                        var y = m[1,3];
                        var z = m[2,3];

                        var sRy = m[0,2];
                        var cRy = Math.Pow((1 - sRy*sRy),0.5);//sign lost

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
                        A = cut_off_2pi(Rx);//-
                        B = cut_off_2pi(Ry);//
                        C = cut_off_2pi(Rz);
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

        static public double cut_off_2pi(double angle)
        {
            if (angle > Math.PI) angle -= 2 * Math.PI;
            else if (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
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

        static public RobotFrame operator *(RobotFrame fr1, RobotFrame fr2)
        {
            var fr = fr1.Clone();
            var m1 = fr1.getMatrix();
            var m2 = fr2.getMatrix();
            fr = new RobotFrame(m1 * m2, fr1.robotType);
            fr.timestamp = fr1.timestamp;
         
            return fr;
        }
        public RobotFrame inv()
        {
            var fr = this.Clone();
            var m1 = fr.getMatrix();
            var m_inv = UtilOpenCV.inv(m1);
            var fr_1 = new RobotFrame(m_inv, fr.robotType);
            fr_1.timestamp = fr.timestamp;

            return fr_1;
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

        public string ToStr(string del = " ",bool nums = false,bool full = true, bool rad = true,bool new_line = true)
        {
            var a = A; var b = B; var c = C;
            if(!rad)
            {
                a = to_degree(A);
                b = to_degree(B);
                c = to_degree(C);
            }
            var str = del + "X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) +
                    del + "A" + round(a) + del + "B" + round(b) + del + "C" + round(c);
            if (full) { str += del + "F" + round(F) + del + "V" + round(V) + del + "D" + D;}
            if (nums)
            {
                str = del + round(X) + del + round(Y) + del + round(Z) +
                    del + round(a) + del + round(b) + del + round(c);
                if (full) { str += del + round(F) + del + round(V) + del + D; }
                
            }
            if (new_line)
            {
                str += " \n";
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
        static double round(double val, double pres = 3)
        {
            return Math.Round(val, 3);
        }
        public  static List<Point3d_GL> to_points(List<RobotFrame> frs)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < frs.Count; i++)
            {
                ps.Add(frs[i].get_pos());
            }
            return ps;
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
                { cos(alpha), -sin(alpha), 0, 0 },
                { sin(alpha),  cos(alpha), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 } });
            return matrix;
        }
        static public Matrix<double> RotYmatr(double alpha)
        {
            var matrix = new Matrix<double>(new double[,] {
                { cos(alpha), 0, sin(alpha), 0 },
                { 0, 1, 0, 0 },
                {-sin(alpha), 0, cos(alpha), 0 },
                { 0, 0, 0, 1 }});
            return matrix;
        }
        static public Matrix<double> RotXmatr(double alpha)
        {
            var matrix = new Matrix<double>(new double[,] {
                { 1, 0, 0, 0 },
                { 0, cos(alpha), -sin(alpha), 0 },
                { 0, sin(alpha),  cos(alpha), 0 },
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
        public static double arcsin(double sin)
        {
            double _sin = sin;
            if (_sin >= 1) _sin = 1;
            if (_sin <= -1) _sin = -1;
            return Math.Asin(_sin);

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
            //Console.WriteLine("prin.t(matrs[0]);");
            //prin.t(matrs[0]);
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
        static double to_rad(double q)
        {
            return q * Math.PI / 180;
        }

        static double to_degree(double q)
        {
            return q *180/ Math.PI;
        }
        static public PositionRob comp_forv_kinem(double[] q,int count, bool rad = true, RobotType robotType = RobotType.PULSE)
        {
            if(robotType==RobotType.PULSE)
            {
                var L1 = 247.1;
                var L2 = 450;
                var L3 = 370;
                var L4 = 135.1;
                var L5 = 182.5;
                var L6 = 134;

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
                //var dh_params_c = new double[6][];
                var dh_params_c = new List<double[]>();
                for (int i=0; i < count;i++)
                {
                    dh_params_c.Add( dh_params[i]);
                }
                var fr = new RobotFrame(calc_pos(dh_params_c.ToArray()), robotType);
                var pos = fr.frame;
                return pos;
            }
            else if(robotType == RobotType.KUKA)
            {
                var dbs = 360.0f;
                var dse = 420.0f;
                var dew = 400.0f;
                var dwf = 126.0f;
                if (!rad)
                    q = to_rad(q);


                var dh_params = new double[][] {
                    new double[]{ q[0], -Math.PI / 2, 0, dbs},
                    new double[]{ q[1], Math.PI / 2, 0, 0},
                    new double[]{ q[2], Math.PI / 2, 0, dse},
                    new double[]{ q[3], -Math.PI / 2, 0, 0},
                    new double[]{ q[4], -Math.PI / 2, 0, dew},
                    new double[]{ q[5], Math.PI / 2, 0, 0 },
                    new double[]{ q[6], 0, 0, dwf }
                };
                //var dh_params_c = new double[6][];
                var dh_params_c = new List<double[]>();
                for (int i = 0; i < count; i++)
                {
                    dh_params_c.Add(dh_params[i]);
                }
                var m = calc_pos(dh_params_c.ToArray());
               // prin.t(q);
               // prin.t("comp_forv_kinem");
               // prin.t(m);
                var fr = new RobotFrame(m, robotType);
               
                var pos = fr.frame;
                pos.m = m;
                return pos;
            }

            return new PositionRob();
        }

        static public double[][] comp_inv_kinem(PositionRob pos, RobotType robotType = RobotType.PULSE,GraphicGL graphic = null)
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
                solvs.Add(comp_inv_kinem_priv(pos, turn,robotType,graphic));

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
        static public double[] comp_inv_kinem_priv(PositionRob pos, int[] turn,RobotType robotType = RobotType.PULSE,GraphicGL graphic = null)
        {
            switch(robotType) 
            {
                case RobotType.KUKA: return comp_inv_kinem_priv_kuka(pos, turn, graphic); 
                case RobotType.PULSE: return comp_inv_kinem_priv_pulse(pos, turn); 
                default: return new double[] { pos.position.x, pos.position.y, pos.position.x, pos.rotation.x, pos.rotation.y, pos.rotation.z };
            }
        }
        static public double[] comp_inv_kinem_priv_pulse(PositionRob pos, int[] turn)
        {


            var pm = getMatrixPos(pos, RobotType.PULSE );
            var p = new Point3d_GL(pm[0, 3], pm[1, 3], pm[2, 3]);
            var L1 = 247.1;
            var L2 = 450;
            var L3 = 370;
            var L4 = 135.1;
            var L5 = 182.5;
            var L6 = 134;

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
            //Console.WriteLine("q");
            for (int i=0 ;i<q.Length;i++){
                var qi = q[i];
                if (qi > Math.PI) qi -= 2 * Math.PI;
                if (qi < -Math.PI) qi += 2 * Math.PI;
                q[i] = qi;
                //Console.WriteLine(q[i]);
            }
            
            return q;
        }

        //---------------------------------------------------------------------
        static public double[] comp_inv_kinem_priv_kuka(PositionRob pos, int[] turn,GraphicGL graphic = null)
        {
            var q = new double[] { 0, 0, 0, 0, 0, 0 ,0,0};

            var pm = getMatrixPos(pos,RobotType.KUKA);
            var m7 = pm;
            var p = new Point3d_GL(pm[0, 3], pm[1, 3], pm[2, 3]);
            var L0 = 360;
            var L1 = 420;
            var L2 = 400;
            var L3 = 126;

            var dz = eye_matr(4);
            dz[2, 3] = -L3;
            var pm3 = pm * dz;
           
            var p6p = new Point3d_GL(pm3[0, 3], pm3[1, 3], pm3[2, 3]);
            var vz = new Point3d_GL(pm[0, 2], pm[1, 2], pm[2, 2]);
            //Console.WriteLine("p3p: " + p6p);
            var scara = p6p - new Point3d_GL(0, 0, L0);
            var sq1 = -scara.y / scara.magnitude_xy();
            var cq1 = -scara.x / scara.magnitude_xy();

            var q1 = Math.Sign(sq1) * arccos(cq1);


            var Ls = scara.magnitude_xy();
            var Lt = scara.magnitude();

            if (Ls == 0) return null;
            if (Lt == 0) return null;
            var theta3 = arccos((L1 * L1 + L2 * L2 - Lt * Lt) / (2 * L1 * L2));
            var q4 = Math.PI - theta3;//Math.PI -

            if (turn[2] < 0)
            {
                q4 *= turn[2];
            }

            var theta_e = Math.Atan(Ls/scara.z );
           //var theta2_sh = arcsin(L2*sin(q3)/Ls);

           var theta2_sh = arccos((L1 * L1 + Lt * Lt - L2 * L2) / (2 * L1 * Lt));

           // theta2 = theta_e - theta2_sh;
            //var omega += omega_ext * turn[2];

            var q2 =-( theta_e- turn[2]*theta2_sh);


            if (turn[0] < 0)
            {
                q1 += Math.PI;
                q2 *= -1;
                //q2 -= Math.PI;
                q4 *= -1;
            }

           
            q[0] = q1;
            q[1] = Math.PI + q2;
            q[2] = 0;
            q[3] = q4;
            //var forv1 = comp_forv_kinem(q, 1, true, RobotType.KUKA);
            //prin.t("matrix1");
            //prin.t((new RobotFrame(forv1)).getMatrix());
            var forv1 = comp_forv_kinem(q, 1, true, RobotType.KUKA);
            var forv3 = comp_forv_kinem(q, 3, true, RobotType.KUKA);
            var m1 = forv1.m;
            var m3 = forv3.m;

            var p7 = new Point3d_GL(m7[0, 3], m7[1, 3], m7[2, 3]);
            var p6 = p6p;
            var p5 = p6p;
            var p4 = new Point3d_GL(m3[0, 3], m3[1, 3], m3[2, 3]);
            var p3 = new Point3d_GL(m3[0, 3], m3[1, 3], m3[2, 3]);
            var p2 = new Point3d_GL(m1[0, 3], m1[1, 3], m1[2, 3]);
            var p1 = new Point3d_GL(m1[0, 3], m1[1, 3], m1[2, 3]);
            var p0 = new Point3d_GL(0,0,0);

            var v0x = new Point3d_GL(1, 0, 0);
            var v0y = new Point3d_GL(0, 1, 0);
            var v0z = new Point3d_GL(0, 0, 1);

            var v7x = new Point3d_GL(m7[0, 0], m7[1, 0], m7[2, 0]);
            var v7y = new Point3d_GL(m7[0, 1], m7[1, 1], m7[2, 1]);
            var v7z = new Point3d_GL(m7[0, 2], m7[1, 2], m7[2, 2]);

            var v6z = v7z.Clone().normalize();
            var v4z = (p6 - p4).Clone().normalize();
            var v6y = (v4z | v7z).Clone().normalize();
            var v5z = v6y.Clone().normalize();
            var v6x = (v6y | v6z).Clone().normalize();
            var v5y = (p4 - p5).Clone().normalize();
            var v5x = (v5y | v5z).Clone().normalize();
            var v3y = (p4 - p2).Clone().normalize();
            var v4y = (v3y | v4z).Clone().normalize();
            var v4x = (v4y | v4z).Clone().normalize();
            var v3z = (-v4y).Clone().normalize(); ;
            var v3x = (v3y | v3z).Clone().normalize();
            var v2z = (p3 - p2).Clone().normalize();
            var v2y = (v0z | v2z).Clone().normalize();
            var v2x = (v2y | v2z).Clone().normalize();
            var v1z = v2y.Clone().normalize(); ;
            var v1y = (-v0z).Clone().normalize();
            var v1x = (v1y | v1z).Clone().normalize();

            var ms = new List<Matrix<double>>();
            ms.Add(matrix_assemble(v0x, v0y, v0z, p0));
            ms.Add(matrix_assemble(v1x, v1y, v1z, p1));
            ms.Add(matrix_assemble(v2x, v2y, v2z, p2));
            ms.Add(matrix_assemble(v3x, v3y, v3z, p3));
            ms.Add(matrix_assemble(v4x, v4y, v4z, p4));
            ms.Add(matrix_assemble(v5x, v5y, v5z, p5));
            ms.Add(matrix_assemble(v6x, v6y, v6z, p6));
            ms.Add(matrix_assemble(v7x, v7y, v7z, p7));

            var ps = new List<Point3d_GL[]>();
            ps.Add(new Point3d_GL[] { p0 });
            ps.Add(new Point3d_GL[] { p1 });
            ps.Add(new Point3d_GL[] { p2 });
            ps.Add(new Point3d_GL[] { p3 });
            ps.Add(new Point3d_GL[] { p4 });
            ps.Add(new Point3d_GL[] { p5 });
            ps.Add(new Point3d_GL[] { p6 });
            ps.Add(new Point3d_GL[] { p7 });

           
            for (int i=0; i<ms.Count; i++)
            {
               // graphic?.addFrame(ms[i], 20,"inv+"+i);
               // graphic?.addPointMesh(ps[i], Color3d_GL.red(), "ps+" + i);
            }
            //var s6 = Point3d_GL.sign_r_v(ax5y, ax6y, ax6z);
            //var q6 = s6 * Point3d_GL.ang(ax5y, ax6y);
            var s5 = Point3d_GL.sign_r_v(v4y, v5z, v4z);
            var q5 = s5 * Point3d_GL.ang(v4y, v5z);//Math.PI - 

            q[4] = q5;
            
            var s6 = Point3d_GL.sign_r_v(v7z, v4z, -v5z);
            var q6 = s6*Point3d_GL.ang(v7z, v4z);//Math.PI - 
            q[5] = q6;
            if (turn[0] < 0)
            {
               // q[5] = -q[5];
            }
            if (turn[1] < 0)
            {
                q[4] -= Math.PI;
                q[5] *= -1;
            }
            if (turn[2] < 0 && turn[0]<0)
            {
                q[5] *= -1;
            }

            if (turn[2] > 0 && turn[0] > 0)
            {
                q[5] *= -1;
            }
            var fm6 = comp_forv_kinem(q, 6, true, RobotType.KUKA).m;

            v6x = new Point3d_GL(fm6[0, 0], fm6[1, 0], fm6[2, 0]);


            var s7 = Point3d_GL.sign_r_v(v6x, v7x, -v7z);
            var q7 = s7*Point3d_GL.ang(v6x, v7x);//Math.PI - 
            q[6] = -q7;
            for (int i = 0; i < q.Length; i++)
            {
                q[i] = cut_off_2pi(q[i]);
            }
            return q;
        }

        static public Matrix<double> matrix_assemble(Point3d_GL vx, Point3d_GL vy, Point3d_GL vz, Point3d_GL p)
        {
          /*  return new Matrix<double>(new double[,] {
            {vx.x,vx.y,vx.z,p.x },
            {vy.x,vy.y,vy.z,p.y },
            {vz.x,vz.y,vz.z,p.z },
            {   0,   0,   0,  1 },
            });*/

            return new Matrix<double>(new double[,] {
            {vx.x,vy.x,vz.x,p.x },
            {vx.y,vy.y,vz.y,p.y },
            {vx.z,vy.z,vz.z,p.z },
            {   0,   0,   0,  1 },
            });
        }
        //--------------------------------------------------------------------------
        static public double[] comp_inv_kinem_priv_kuka_2(PositionRob pos, int[] turn)
        {
            var q = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            var pm = getMatrixPos(pos, RobotType.KUKA);
            var p = new Point3d_GL(pm[0, 3], pm[1, 3], pm[2, 3]);
            var L0 = 360;
            var L1 = 420;
            var L2 = 400;
            var L3 = 126;

            var dz = eye_matr(4);
            dz[2, 3] = -L3;
            var p3 = pm * dz;

            var p3p = new Point3d_GL(p3[0, 3], p3[1, 3], p3[2, 3]);
            var vz = new Point3d_GL(pm[0, 2], pm[1, 2], pm[2, 2]);
            Console.WriteLine("p3p: " + p3p);
            var scara = p3p - new Point3d_GL(0, 0, L0);
            var sq1 = -scara.y / scara.magnitude_xy();
            var cq1 = -scara.x / scara.magnitude_xy();

            var q1 = Math.Sign(sq1) * arccos(cq1);


            var Ls = scara.magnitude_xy();
            var Lt = scara.magnitude();

            if (Ls == 0) return null;
            if (Lt == 0) return null;
            var theta3 = arccos((L1 * L1 + L2 * L2 - Lt * Lt) / (2 * L1 * L2));
            var q4 = Math.PI - theta3;//Math.PI -

            if (turn[2] < 0)
            {
                q4 *= turn[2];
            }

            var theta_e = Math.Atan(Ls / scara.z);
            //var theta2_sh = arcsin(L2*sin(q3)/Ls);

            var theta2_sh = arccos((L1 * L1 + Lt * Lt - L2 * L2) / (2 * L1 * Lt));

            // theta2 = theta_e - theta2_sh;
            //var omega += omega_ext * turn[2];

            var q2 = -(theta_e - turn[2] * theta2_sh);


            if (turn[0] < 0)
            {
                q1 += Math.PI;
                q2 *= -1;
                //q2 -= Math.PI;
                q4 *= -1;
            }


            q[0] = q1;
            q[1] = Math.PI + q2;
            q[2] = 0;
            q[3] = q4;
            //var forv1 = comp_forv_kinem(q, 1, true, RobotType.KUKA);
            //prin.t("matrix1");
            //prin.t((new RobotFrame(forv1)).getMatrix());
            var forv3 = comp_forv_kinem(q, 3, true, RobotType.KUKA);
            var forv4 = comp_forv_kinem(q, 4, true, RobotType.KUKA);
            
            //prin.t("matrix4");
            var m4 = forv4.m;
            var m3 = forv3.m;
            prin.t((new RobotFrame(forv4)).getMatrix());

            var ax4x = new Point3d_GL(m4[0, 0], m4[1, 0], m4[2, 0]);
            var ax4y = new Point3d_GL(m4[0, 1], m4[1, 1], m4[2, 1]);
            var ax4z = new Point3d_GL(m4[0, 2], m4[1, 2], m4[2, 2]);

            var ax5z = ax4y;
            var q6 = Point3d_GL.ang(vz, ax4z);//Math.PI - 
            q[5] = q6;
            if (turn[0] > 0)
            {
                q[5] = -q[5];
            }
            Console.WriteLine("q6: " + q6);
            Console.WriteLine(turn[0] + " " + turn[1] + " " + turn[2] + " ");
            var ax3z = new Point3d_GL(m3[0, 2], m3[1, 2], m3[2, 2]);
            var ax_wr = ax4z | vz;

            var q5 = Point3d_GL.ang(ax4y, ax_wr);//Math.PI - 

            q[4] = q5;
            if (turn[1] < 0)
            {
                q[4] -= Math.PI;
                q[5] *= -1;
            }
            var forv6 = comp_forv_kinem(q, 6, true, RobotType.KUKA);
            var m6 = forv6.m;
            var ax6z = new Point3d_GL(m6[0, 2], m6[1, 2], m6[2, 2]);
            var ax6x = new Point3d_GL(m6[0, 0], m6[1, 0], m6[2, 0]);
            var ax7x = new Point3d_GL(pm[0, 0], pm[1, 0], pm[2, 0]);
            var q7 = Point3d_GL.ang(ax6x, ax7x);//Math.PI - 
            q[6] = -q7;

            for (int i = 0; i < q.Length; i++)
            {
                q[i] = cut_off_2pi(q[i]);
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
