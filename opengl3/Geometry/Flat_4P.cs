using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
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
            foreach (var P in _p)
            {
                ps.Add(P);
            }
            P = ps.ToArray();
            verifigateSignature();
        }
        private void verifigateSignature()
        {

            if (F[1].valP(P[3]) < 0)
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
        bool checkInside(Point3d_GL p, int ind)
        {
            bool ret = true;
            for (int i = 0; i < 3; i++)
            {
                if (F[ind + i].valP(p) < -20)
                {
                    return false;
                }
            }
            return ret;
        }
        public Point3d_GL crossLine(Line3d_GL line)
        {
            var pCposs = line.calcCrossFlat(F[0]);

            if (checkInside(pCposs, 1))
            {
                return pCposs;
            }
            pCposs = line.calcCrossFlat(F[4]);
            if (checkInside(pCposs, 5))
            {
                return pCposs;
            }

            return Point3d_GL.notExistP();
        }
    }
}
