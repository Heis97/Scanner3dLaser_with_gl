using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    class RasterMap
    {
        int[,][] map;
        int[,,][] map_xyz;
        double res;
        Point3d_GL pt_min, pt_max;
        public enum type_map { XY,XYZ};
        public RasterMap(Polygon3d_GL[] surface, double resolution,type_map type = type_map.XY)
        {
            switch (type)
            {
                case type_map.XY: rasterxy_surface_xy(surface, resolution); break;
                case type_map.XYZ: rasterxy_surface_xyz(surface, resolution); break;
                default: break;
            }
           
        }
        void rasterxy_surface_xy(Polygon3d_GL[] surface, double resolution)
        {
            var p_minmax = Polygon3d_GL.get_dimens_minmax_arr(surface);
            var p_min = p_minmax[0]; var p_max = p_minmax[1];
            var p_len = (p_max - p_min) / resolution;
            var x_len = (int)(p_len.x * 1.1);
            var y_len = (int)(p_len.y * 1.1);
            var map_xy = new int[x_len, y_len][];

            int triangle_overlay = 1;
            for (int i = 0; i < surface.Length; i++)
            {
                var pol_minmax = surface[i].get_dimens_minmax();
                var pol_min = pol_minmax[0] - p_min; var pol_max = pol_minmax[1] - p_min;
                for (int x = (int)(pol_min.x / resolution)- triangle_overlay; x < pol_max.x / resolution+ triangle_overlay; x++)
                {
                    for (int y = (int)(pol_min.y / resolution)- triangle_overlay; y < pol_max.y / resolution+ triangle_overlay; y++)
                    {
                        if (x >= x_len) x = x_len-1;
                        if (x < 0) x = 0;

                        if(y >= y_len) y = y_len - 1;
                        if (y < 0) y = 0;

                        if (map_xy[x, y] == null) map_xy[x, y] = new int[0];
                        var map_cur = map_xy[x, y];
                        var list = map_cur.ToList();
                        list.Add(i);
                        map_xy[x, y] = list.ToArray();
                    }
                }
            }
            map = map_xy;
            pt_min = p_min;
            pt_max = p_max;
            res = resolution;
        }

        void rasterxy_surface_xyz(Polygon3d_GL[] surface, double resolution)
        {
            var p_minmax = Polygon3d_GL.get_dimens_minmax_arr(surface);
            var p_min = p_minmax[0]; var p_max = p_minmax[1];
            var p_len = (p_max - p_min) / resolution;
            var x_len = (int)(p_len.x * 1.1);
            var y_len = (int)(p_len.y * 1.1);
            var z_len = (int)(p_len.z * 1.1);
            var map_xyz = new int[x_len, y_len, z_len][];

            int triangle_overlay = 1;
            for (int i = 0; i < surface.Length; i++)
            {
                var pol_minmax = surface[i].get_dimens_minmax();
                var pol_min = pol_minmax[0] - p_min; var pol_max = pol_minmax[1] - p_min;
                for (int x = (int)(pol_min.x / resolution) - triangle_overlay; x < pol_max.x / resolution + triangle_overlay; x++)
                {
                    for (int y = (int)(pol_min.y / resolution) - triangle_overlay; y < pol_max.y / resolution + triangle_overlay; y++)
                    {
                        for (int z = (int)(pol_min.z / resolution) - triangle_overlay; z < pol_max.z / resolution + triangle_overlay; z++)
                        {
                            if (x >= x_len) x = x_len - 1;
                            if (x < 0) x = 0;

                            if (y >= y_len) y = y_len - 1;
                            if (y < 0) y = 0;

                            if (z >= z_len) y = z_len - 1;
                            if (z < 0) z = 0;

                            if (map_xyz[x, y,z] == null) map_xyz[x, y, z] = new int[0];
                            var map_cur = map_xyz[x, y, z];
                            var list = map_cur.ToList();
                            list.Add(i);
                            map_xyz[x, y, z] = list.ToArray();
                        }
                    }
                }
            }
            this.map_xyz = map_xyz;
            pt_min = p_min;
            pt_max = p_max;
            res = resolution;
        }

        public int get_polyg_ind(Point3d_GL p)
        {
            var p_xy = (p - pt_min) / res;
            var x = (int)p_xy.x;
            var y = (int)p_xy.y;
            
            
            var inds = map[x, y];
            if(inds!=null)
                if (inds.Length > 0)
                {
                    return inds[0];
                }
            return 0;
        }

        public int get_polyg_ind_prec_xy(Point3d_GL p,Polygon3d_GL[] surface)
        {
            if (map == null) return 0;
            var p_xy = (p - pt_min) / res;
            var x = (int)p_xy.x;
            var y = (int)p_xy.y;


            var inds = map[x, y];
            if (inds != null)
            {
                if (inds.Length > 0)
                {
                    for (int i = 0; i < inds.Length; i++)
                    {
                        if (surface[inds[i]].affilationPoint_xy(p))
                        {
                            return inds[i];
                        }
                    }
                    return inds[0];
                }
            }
            return 0;
        }

        public int get_polyg_ind_prec_xyz(Point3d_GL p, Polygon3d_GL[] surface)
        {
            if (map_xyz == null) return 0;
            var p_xy = (p - pt_min) / res;
            var x = (int)p_xy.x;
            var y = (int)p_xy.y;
            var z = (int)p_xy.z;

            var inds = map_xyz[x, y, z];
            if (inds != null)
            {
                if (inds.Length > 0)
                {
                    for (int i = 0; i < inds.Length; i++)
                    {
                        if (surface[inds[i]].affilationPoint_xy(p))
                        {
                            return inds[i];
                        }
                    }
                    return inds[0];
                }
            }
            return 0;
        }
        public enum type_out { inside, outside };
        public Polygon3d_GL[] get_polyg_contour_xy(Point3d_GL[] cont, Polygon3d_GL[] surface, type_out type)
        {
            if (map == null) return null;
            
            var map_board = board_map_xy(cont);
            //UtilOpenCV.showMap(map_board);
            var inds = inds_xy(map_board);
            //UtilOpenCV.showInds(inds);
            var  s_ind = new List<int>();
            foreach (var ind_xy in inds)
            {
                if(map[ind_xy[0], ind_xy[1]]!=null)
                    s_ind.AddRange(map[ind_xy[0], ind_xy[1]]);
            }
            var pols = new List<Polygon3d_GL>();


            var s_ind_a = s_ind.Distinct().ToArray();

            var s_ind_b = new int[surface.Length];
            for(int i = 0; i < s_ind_b.Length; i++) s_ind_b[i] = i;
            s_ind_b = s_ind_b.Except(s_ind_a).ToArray();


            if (type == type_out.inside)            
                for (int i = 0; i < s_ind_a.Length; i++)                                
                    pols.Add(surface[s_ind_a[i]]);
                                         
            if (type == type_out.outside)
                for (int i = 0; i < s_ind_b.Length; i++)
                    pols.Add(surface[s_ind_b[i]]);


            return pols.ToArray();
        }

        int[][] inds_xy(int[,] board_map_xy)
        {
            var inds = new List<int[]>();
            for (int i = 0; i < board_map_xy.GetLength(0); i++)
            {
                int start = 0;
                int stop = 0;
                for(int j = 0; j < board_map_xy.GetLength(1); j++)
                {
                    if(board_map_xy[i, j] != 0)
                    {
                        start = j;
                        for (int k = board_map_xy.GetLength(1)-1; k >=0; k--)
                        {
                            if (board_map_xy[i, k] != 0)
                            {
                                stop = k;
                                break;
                            }
                            
                        }
                        break;
                    }
                }

                if (start < stop)
                {
                    for(int j = start; j <= stop; j++)
                    {
                        inds.Add(new int[] {i, j});
                    }
                }
            }
            return inds.ToArray();
        }
        int[,] board_map_xy(Point3d_GL[] cont)
        {
            cont = Point3d_GL.order_points(cont);
            var ps_d = ps_to_discr_xy(cont);
            return continues_cont(ps_d);
        }
        int[][] ps_to_discr_xy(Point3d_GL[] ps)
        {
            var ps_d = new List<int[]>();
            for(int i=0; i < ps.Length; i++)
            {
                var p_xy = (ps[i] - pt_min) / res;
                ps_d.Add(new int[] { (int)p_xy.x, (int)p_xy.y });
            }
            return ps_d.ToArray();
        }

        int[,] continues_cont(int[][] ps_d)
        {
            var max_p = max_v(ps_d);
            var map_f = new int[max_p[0]+1, max_p[1]+1];
            for(int i=0; i < ps_d.Length; i++)
            {
                int beg = i - 1;
                if (beg < 0) beg = ps_d.Length - 1;
                var ps_l = fill_line(ps_d[beg], ps_d[i]);
                for(int j=0; j < ps_l.Length; j++)
                {

                    if (ps_l[j][0] >= map_f.GetLength(0)) ps_l[j][0] = map_f.GetLength(0) - 1;
                    if (ps_l[j][1] >= map_f.GetLength(1)) ps_l[j][1] = map_f.GetLength(1) - 1;
                    if (ps_l[j][0] < 0) ps_l[j][0] = 0;
                    if (ps_l[j][1] < 0) ps_l[j][1] = 0;

                    map_f[ps_l[j][0], ps_l[j][1]] = 1;
                }                
            }
            return map_f;
        }

        int[] max_v(int[][] ps)
        {
            var max_p = new int[2];
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i][0] > max_p[0]) max_p[0] = ps[i][0];
                if (ps[i][1] > max_p[1]) max_p[1] = ps[i][1];
            }
            return max_p;
        }

        int[][] fill_line(int[] p1, int[] p2)
        {
            var dx = p2[0] - p1[0];
            var dy = p2[1] - p1[1];
            var line = new List<int[]>();
            line.Add(p1);
            if (Math.Abs(dx)> Math.Abs(dy) && Math.Abs(dx)>0)
            {
                int start = 1;
                int stop = dx;
                if (dx < 0)
                {
                    start = dx;
                    stop = 0;
                } 
                for(int x= start; x< stop; x++)
                {
                    var y = (int)((double)(x) * ((double)dy / (double)dx));// + p1[1];
                    line.Add(new int[] {p1[0] + x, p1[1] + y });
                }
            }
            else if (Math.Abs(dy) >= Math.Abs(dx) && Math.Abs(dy) > 0)
            {
                int start = 1;
                int stop = dy;
                int yd = 1;
                if (dy < 0)
                {
                    yd = -1;
                    start = dy;
                    stop = 0;
                }
                for (int y = start; y < stop; y++)
                {
                    var x = (int)((double)y * ((double)dx / (double)dy));// + p1[0];
                    line.Add(new int[] { p1[0] + x , p1[1] + y });
                }
            }
            line.Add(p2);
            return line.ToArray();
        }
    }
}
