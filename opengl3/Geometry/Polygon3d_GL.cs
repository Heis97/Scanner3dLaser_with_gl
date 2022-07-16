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
            //Console.WriteLine("intersect " + polygons.Length +" polyg and " + lines.Length + " lines: " + ps_laser.Count + " points");
            return ps_laser.ToArray();
        }

        static Point3d_GL[] sortByX(Point3d_GL[] ps)
        {
            var ps_sort = from p in ps
                            orderby p.x
                            select p;
            return ps_sort.ToArray();
        }

        static public Polygon3d_GL[] triangulate_two_lines_xy(Point3d_GL[] _ps1, Point3d_GL[] _ps2)
        {
            var ps1 = sortByX(_ps1).Reverse().ToArray();
            var ps2 = sortByX(_ps2).Reverse().ToArray();
            var polygons = new List<Polygon3d_GL>();
            int ind_2 = 0;
            List<int>[] ps1_connect = new List<int>[ps1.Length];
            for(int i=1; i < ps1.Length; i++)
            {
                polygons.Add(new Polygon3d_GL(ps1[i-1],ps1[i],ps2[ind_2]));
                if (ps1_connect[i-1] == null)
                {
                    ps1_connect[i-1] = new List<int>();
                }
                if (ps1_connect[i]==null)
                {
                    ps1_connect[i] = new List<int>();
                }
                ps1_connect[i-1].Add(ind_2);
                ps1_connect[i].Add(ind_2);
                if(i<ps1.Length-1)
                {
                  
                    var dist1 = (ps1[i + 1] - ps2[ind_2]).magnitude_xy();
                    var dist2 = (ps1[i] - ps2[ind_2]).magnitude_xy();
                   // Console.WriteLine();
                    if ((dist1 > dist2) && ind_2<ps2.Length-1)
                    {
                        //Console.WriteLine("ind_2: " + ind_2);
                        ind_2++;
                    }
                }
                
            }

            for(int i=0; i < ps1_connect.Length; i++)
            {
                if(ps1_connect[i]!=null)
                {
                    if(ps1_connect[i].Count>1)
                    {
                        polygons.Add(new Polygon3d_GL(ps2[ps1_connect[i][1]], ps2[ps1_connect[i][0]], ps1[i] ));
                    }
                }
            }
            
            return polygons.ToArray();
        }

        static public Polygon3d_GL[] triangulate_lines_xy(Point3d_GL[][] ps)
        {
            List<Polygon3d_GL> polygons = new List<Polygon3d_GL>();
            for(int i=1; i<ps.Length; i++)
            {
                polygons.AddRange(triangulate_two_lines_xy(ps[i - 1], ps[i]));
            }
            return polygons.ToArray();
        }


        static public float[][] toMesh(Polygon3d_GL[] polygons)
        {
            var mesh =new  List<float>();
            var color = new List<float>();
            var normal = new List<float>();
            for (int i=0; i<polygons.Length;i++)
            {               
                if(polygons[i].ps.Length>2)
                {
                    var mesh_sub = new float[9];
                    mesh_sub[0] = (float)polygons[i].ps[0].x; 
                    mesh_sub[1] = (float)polygons[i].ps[0].y; 
                    mesh_sub[2] = (float)polygons[i].ps[0].z; 

                    mesh_sub[3] = (float)polygons[i].ps[1].x;
                    mesh_sub[4] = (float)polygons[i].ps[1].y; 
                    mesh_sub[5] = (float)polygons[i].ps[1].z;

                    mesh_sub[6] = (float)polygons[i].ps[2].x;
                    mesh_sub[7] = (float)polygons[i].ps[2].y;
                    mesh_sub[8] = (float)polygons[i].ps[2].z;
                    mesh.AddRange(mesh_sub);


                    var color_sub = new float[9];
                    if (polygons[i].ps[0].color!= null && polygons[i].ps[1].color != null && polygons[i].ps[2].color != null)
                    {
                        color_sub[0] = (float)polygons[i].ps[0].color.r;
                        color_sub[1] = (float)polygons[i].ps[0].color.g;
                        color_sub[2] = (float)polygons[i].ps[0].color.b;

                        color_sub[3] = (float)polygons[i].ps[1].color.r;
                        color_sub[4] = (float)polygons[i].ps[1].color.g;
                        color_sub[5] = (float)polygons[i].ps[1].color.b;

                        color_sub[6] = (float)polygons[i].ps[2].color.r;
                        color_sub[7] = (float)polygons[i].ps[2].color.g;
                        color_sub[8] = (float)polygons[i].ps[2].color.b;
                    }
                        
                    color.AddRange(color_sub);

                    var normal_sub = new float[9];
                    normal_sub[0] = (float)polygons[i].flat3D.A;
                    normal_sub[1] = (float)polygons[i].flat3D.B;
                    normal_sub[2] = (float)polygons[i].flat3D.C;

                    normal_sub[3] = (float)polygons[i].flat3D.A;
                    normal_sub[4] = (float)polygons[i].flat3D.B;
                    normal_sub[5] = (float)polygons[i].flat3D.C;

                    normal_sub[6] = (float)polygons[i].flat3D.A;
                    normal_sub[7] = (float)polygons[i].flat3D.B;
                    normal_sub[8] = (float)polygons[i].flat3D.C;
                    normal.AddRange(normal_sub);
                }              
            }
            return new float[][] { mesh.ToArray(), color.ToArray(), normal.ToArray() };
        }        
    }
}
