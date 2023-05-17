using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace opengl3
{
    public class RobotFrame
    {
        public double X, Y, Z, A, B, C, V, D;
        public enum RobotType { KUKA = 1, PULSE = 2};

        public RobotType robotType;


        public RobotFrame(double x, double y, double z, double a, double b, double c, double v, double d, RobotType robotType)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            V = v;
            D = d;
            this.robotType = robotType;
        }

        public RobotFrame(string coords)
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
            this.robotType = RobotType.PULSE;
        }

        public Matrix<double>  getMatrix()
        {
            return ABCmatr(X, Y, Z, A, B, C);
        }

        public RobotFrame(Matrix<double> m, double v, RobotType type = RobotType.KUKA)
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
                        B = Ry;
                        C = Rx;
                        V = v;
                        D = d;
                    }
                    break;
                case RobotType.PULSE:
                    {
                        var x = m[0,3];
                        var y = m[1,3];
                        var z = m[2,3];

                        var sRy = m[0,2];
                        var cRy = Math.Pow(Math.Pow( (1 - sRy),2),0.5);

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
                        V = v;
                        D = d;
                    }
                    break;
            }            
        }

        public static double arccos(double cos)
        {
            double _cos = cos;
            if (_cos >= 1) _cos = 1;
            if (_cos <= -1) _cos = -1;
            return Math.Acos(_cos);

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

        public override string ToString()
        {
            return " X" + round(X) + ", Y" + round(Y) + ", Z" + round(Z) +
                    ", A" + round(A) + ", B" + round(B) + ", C" + round(C) +
                    ", V" + round(V) + ", D" + D + " \n";
        }

        public string ToStr(string del = " ")
        {
            return del+"X" + round(X) + del + "Y" + round(Y) + del + "Z" + round(Z) +
                    del + "A" + round(A*0.5) + del + "B" + round(B*0.5) + del + "C" + round(C) +
                    del + "V" + round(V) + del + "D" + D + " \n";
        }

        static double round(double val)
        {
            return Math.Round(val, 4);
        }

        public RobotFrame Clone()
        {
            return new RobotFrame(X, Y, Z, A, B, C, V, D,robotType);
        }
        //---------------------------------------------------------------------------------------

        static public  List<RobotFrame> smooth_angle(List<RobotFrame> frames, int w)
        {
            var smooth_frames = new List<RobotFrame>();
            for (int i = 0; i < frames.Count; i++)
            {
                int start_i = i - w;
                int count =  2*w;
                if(start_i < 0) start_i = 0;
                if(start_i + count > frames.Count) count = frames.Count - 1 - start_i;
                var wind = frames.GetRange(start_i, count);
                smooth_frames.Add(get_average_angle(wind, frames[i].Clone()));
            }
            return smooth_frames;
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
        static public Matrix<double> ABCmatr(double X, double Y, double Z, double A, double B, double C)
        {
            var matrix = Translmatr(X, Y, Z)* RotZmatr(A) * RotYmatr(B) * RotXmatr(C);
            return matrix;
        }

        static public double cos(double alpha)
        {
            return Math.Cos(alpha);
        }
        static public double sin(double alpha)
        {
            return Math.Sin(alpha);
        }
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
