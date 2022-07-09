using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Polygon3d_GL
    {
        public Point3d_GL[] ps;
        public Flat3d_GL flat3D;
        public Vector3d_GL v1,v2,v3;
        int special_point_ind;

        public Polygon3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3, int _special_point_ind = 0)
        {
            special_point_ind = _special_point_ind;
            ps = new Point3d_GL[] { P1, P2, P3};   
            v1 = new Vector3d_GL(P1, P2);
            v2 = new Vector3d_GL(P1, P3);
            v3 = v1 | v2;//vector multiply
            v3.normalize();
            flat3D = new Flat3d_GL(v3.x, v3.y, v3.z, -v3 * P1);
        }

        static public Flat3d_GL notExistFlat()
        {
            var flat = new Flat3d_GL();
            flat.exist = false;
            return flat;
        }

        public bool affilationPoint_xy(Point3d_GL p)
        {
            if (ps.Length < 3)
            {
                return false;
            }

            p = p - ps[0];
            var b = ps[1] - ps[0];
            var c = ps[2] - ps[0];

            var m = (p.x * b.y - b.x * p.y) / (c.x * b.y - b.x * c.y);
            if (m >= 0 && m <= 1)
            {
                var l = (p.x - m * c.x) / b.x;
                if (l >= 0 && m + l <= 1)
                {
                    return true;
                }                   
            }                
            return false;
        }

        public Point3d_GL crossLine(Line3d_GL p1)
        {
            var p_cross = p1.calcCrossFlat(flat3D);
            var v_c = new Vector3d_GL(ps[special_point_ind],p_cross);
            var a1 = v1 ^ v_c;
            var a2 = v2 ^ v_c;
            var b1 = v1 ^ v2;
            if (a1 <= b1 && a2 <= b1)
            {
                return p_cross;
            }
            return Point3d_GL.notExistP();
        }


        static public Point3d_GL[] createLightFlat(Polygon3d_GL[] polygons, Line3d_GL[] lines)
        {
            var ps_laser = new List<Point3d_GL>();
            for(int line_i=0; line_i < lines.Length; line_i++)
            {
                for (int polygon_i = 0; polygon_i < polygons.Length; polygon_i++)
                {
                    var p = polygons[polygon_i].crossLine(lines[line_i]);
                    if(p.exist)
                    {
                        ps_laser.Add(p);
                    }
                }
            }

            return ps_laser.ToArray();
        }


    }
}
