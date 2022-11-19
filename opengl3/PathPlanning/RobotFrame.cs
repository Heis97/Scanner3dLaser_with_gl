using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace opengl3
{
    class RobotFrame
    {
        public double X, Y, Z, A, B, C, V, D;
        
        public RobotFrame(double x, double y, double z, double a, double b, double c, double v, double d)
        {
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            V = v;
            D = d;
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
        }

        public Matrix<double>  getMatrix()
        {
            return ABCmatr(X, Y, Z, A, B, C);
        }

        public RobotFrame(Matrix<double> matrix, double v)
        {
            Matrix<double> base_matrix = ABCmatr(596.56, 87.9, 57.0, 1.9, 0.01, -0.005);
            //var matrix = mult_frame(base_matrix, _matrix);
            // CvInvoke.Invert(base_matrix, base_matrix, Emgu.CV.CvEnum.DecompMethod.LU);
            // CvInvoke.Invert(_matrix, _matrix, Emgu.CV.CvEnum.DecompMethod.LU);
           // var matrix = base_matrix*_matrix;
           /* var trans = base_matrix * _matrix;
            

            matrix[0, 3] = trans[0, 3];
            matrix[1, 3] = trans[1, 3];
            matrix[2, 3] = trans[2, 3];*/
            //prin.t(base_matrix);
            /* 
             var vecs = toVcs(_matrix);
             prin.t(base_matrix);
             prin.t(vecs);
             var vecs_matr = vecs * base_matrix;
             var matrix =toVcs(vecs_matr);*/
             prin.t(base_matrix);
             //prin.t(_matrix);

             prin.t(matrix);
             prin.t("___________");
            //var matrix = base_matrix * _matrix;
            //CvInvoke.Invert(base_matrix, base_matrix, Emgu.CV.CvEnum.DecompMethod.LU);

            var x = matrix[0, 3];
            var y = matrix[1, 3];
            var z = matrix[2, 3];
            double b = Math.Asin(-matrix[2, 0]);
            double a = 0;
            double c = 0;
            if (Math.Cos(b) != 0)
            {
                c = Math.Asin(matrix[2, 1] / Math.Cos(b));
                a = Math.Asin(matrix[1, 0] / Math.Cos(b));
            }
            int d = (int)matrix[3, 3];
            X = x;
            Y = y;
            Z = z;
            A = a;
            B = b;
            C = c;
            V = v;
            D = d;
        }

        static public string generate_string(RobotFrame[] frames)
        {
            var traj_rob = new StringBuilder();
            for (int i = 0; i < frames.Length; i++)
            {
                traj_rob.Append(frames[i].ToString());
            }
            traj_rob.Append("q\n");
            return traj_rob.ToString();
        }

        public override string ToString()
        {
            return " X" + round(X) + ", Y" + round(Y) + ", Z" + round(Z) +
                    ", A" + round(A) + ", B" + round(B) + ", C" + round(C) +
                    ", V" + round(V) + ", D" + D + " \n";
        }

        static double round(double val)
        {
            return Math.Round(val, 4);
        }

        public RobotFrame Clone()
        {
            return new RobotFrame(X, Y, Z, A, B, C, V, D);
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
