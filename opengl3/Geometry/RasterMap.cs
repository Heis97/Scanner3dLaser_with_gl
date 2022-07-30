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
        double res;
        Point3d_GL pt_min, pt_max;
        public RasterMap(Polygon3d_GL[] surface, double resolution)
        {
            rasterxy_surface(surface, resolution);
        }
        void rasterxy_surface(Polygon3d_GL[] surface, double resolution)
        {
            var p_minmax = Polygon3d_GL.get_dimens_minmax_arr(surface);
            var p_min = p_minmax[0]; var p_max = p_minmax[1];
            var p_len = (p_max - p_min) / resolution;
            var x_len = (int)(p_len.x * 1.1);
            var y_len = (int)(p_len.y * 1.1);
            var map_xy = new int[x_len, y_len][];

            int triangle_overlay = 0;
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

        public int get_polyg_ind_prec(Point3d_GL p,Polygon3d_GL[] surface)
        {
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
    }
}
