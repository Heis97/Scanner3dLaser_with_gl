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

        public RobotFrame(Matrix<double> matrix, double v)
        {
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
                    ", A" + round(A) + ", B" + round(0.5*C) + ", C" + round(0.5 * B) +
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
    }
}
