using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class LineF
    {
        public float X;
        public float Y;
        public PlanType plan;
        public LineF(float _x, float _y, PlanType _plan = PlanType.X)
        {
            X = _x;
            Y = _y;
            plan = _plan;
        }
        public LineF calcLine(PointF[] points, PlanType _plan = PlanType.X, int maxPoints = 1000)
        {

            int n = points.Length;
            //Console.WriteLine(n);
            if (n > maxPoints || n < 2)
            {
                return null;
            }
            double x = 0;
            double y = 0;
            double xy = 0;
            double xx = 0;
            float X = 0;
            float Y = 0;
            foreach (var p in points)
            {
                if (_plan == PlanType.X)
                {
                    X = p.X;
                    Y = p.Y;
                }
                else
                {
                    X = p.Y;
                    Y = p.X;
                }
                x += X;
                y += Y;
                xy += X * Y;
                xx += X * X;
            }
            var del = (n * xx - x * x);
            if (Math.Abs(del) < 0.1)
            {
                if (_plan == PlanType.X)
                {
                    return calcLine(points, PlanType.Y);
                }
                if (_plan == PlanType.Y)
                {
                    return calcLine(points, PlanType.X);
                }
                //return new PointF(4000, (float)x/(float)n);
            }
            float a = (float)((n * xy - x * y) / del);

            float b = (float)((y - a * x) / n);
            if (Math.Abs(a) > 1.1)
            {
                if (_plan == PlanType.X)
                {
                    return calcLine(points, PlanType.Y);
                }
                if (_plan == PlanType.Y)
                {
                    return calcLine(points, PlanType.X);
                }
                //return new PointF(4000, (float)x/(float)n);
            }
            if (_plan == PlanType.X)
            {
                //Console.WriteLine("Line plan X " + a +" "+b+ " " + del);
            }
            else
            {
                //Console.WriteLine("Line plan Y " + a + " " + b);
            }
            return new LineF(a, b, _plan);
        }
        public PointF findCrossing(LineF L1, LineF L2)
        {

            if (L1.plan == PlanType.X && L2.plan == PlanType.X)
            {
                var x = (L2.Y - L1.Y) / (L1.X - L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" X - X");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.X && L2.plan == PlanType.Y)
            {
                var x = (L1.Y * L2.X + L2.Y) / (1 - L1.X * L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" X - Y");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.Y && L2.plan == PlanType.X)
            {
                var x = (L2.Y * L1.X + L1.Y) / (1 - L2.X * L1.X);
                var y = L2.X * x + L2.Y;
                // Console.WriteLine(" Y - X");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.Y && L2.plan == PlanType.Y)
            {

                var x = (L2.Y - L1.Y) / (L1.X - L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" Y - Y");
                return new PointF(y, x);
            }
            else
            {
                return PointF.notExistP() ;
            }
        }

    }
}
