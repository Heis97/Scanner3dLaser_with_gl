using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class PosTimestamp
    {
        public double pos1 { get; set; }
        public double pos2 { get; set; }
        public double pos3 { get; set; }
        public long time { get; set; }
        public PosTimestamp(long _time = 0, double _pos1 = 0, double _pos2 = 0, double _pos3 = 0)
        {
            pos3 = _pos3;
            pos2 = _pos2;
            pos1 = _pos1;
            time = _time;
        }
        static public PosTimestamp betw(PosTimestamp p1, PosTimestamp p2, long time)
        {
            if(p2.time - p1.time==0) return p1;
            var k1 = (p2.pos1 - p1.pos1) / (p2.time - p1.time);
            var k2 = (p2.pos2 - p1.pos2) / (p2.time - p1.time);
            var k3 = (p2.pos3 - p1.pos3) / (p2.time - p1.time);
            var pos1i = k1 * (time - p1.time)+ p1.pos1;
            var pos2i = k2 * (time - p1.time)+ p1.pos2;
            var pos3i = k3 * (time - p1.time)+ p1.pos3;
            return new PosTimestamp(time, pos1i,pos2i,pos3i);
        }
        public static List<PosTimestamp> line_aver(List<PosTimestamp> ps, int wind)
        {
            if (ps == null) return null;
            if (ps.Count < 3) return null;
            var ps_s = new List<PosTimestamp>();
            for (int i = 0; i < ps.Count; i++)
            {
                int beg = i - wind; if (beg < 0) beg = 0;
                int end = i + wind; if (end > ps.Count - 1) end = ps.Count - 1;
                var ps_av = new PosTimestamp();
                for (int j = beg; j < end; j++)
                {
                    ps_av += ps[j];
                }
                ps_s.Add( ps_av / (end - beg));
            }

            return ps_s;
        }

        public static List<PosTimestamp> arr_minus(List<PosTimestamp> ps1, List<PosTimestamp> ps2)
        {
            if (ps1 == null || ps2 == null) return null;
            var len = Math.Min(ps1.Count, ps2.Count);   
            var ps_minus = new List<PosTimestamp>();
            for (int i = 0; i < len; i++)
            {
                ps_minus.Add(ps1[i]-ps2[i]);
            }
            return ps_minus;
        }

        public static PosTimestamp arr_summ_abs(List<PosTimestamp> ps)
        {
            if (ps == null) return null;

            var p_sum = new PosTimestamp();
            for (int i = 0; i < ps.Count; i++)
            {
                p_sum+= ps[i].abs();
            }

            return p_sum;
        }
        public static PosTimestamp operator +(PosTimestamp p1, PosTimestamp p2)
        {
            return new PosTimestamp(p1.time + p2.time, p1.pos1 + p2.pos1, p1.pos2 + p2.pos2, p1.pos3 + p2.pos3);
        }
        public static PosTimestamp operator -(PosTimestamp p1, PosTimestamp p2)
        {
            return new PosTimestamp(p1.time - p2.time, p1.pos1 - p2.pos1, p1.pos2 - p2.pos2, p1.pos3 - p2.pos3);
        }
        public static PosTimestamp operator *(PosTimestamp p1, double k)
        {
            return new PosTimestamp((long)(p1.time *k), p1.pos1 * k, p1.pos2 * k, p1.pos3 * k);
        }
        public static PosTimestamp operator /(PosTimestamp p1, double k)
        {
            return new PosTimestamp((long)(p1.time / k), p1.pos1 / k, p1.pos2 / k, p1.pos3 / k);
        }
        public PosTimestamp abs()
        {
            return new PosTimestamp(Math.Abs(time), Math.Abs(pos1), Math.Abs(pos2), Math.Abs(pos3));
        }
        public override string ToString()
        {
            return time + " " + pos1 + " " + pos2 + " " + pos3;
        }
    }
    public class MovmentCompensation
    {
        long dt;
        long start_time;
        long period;
        List<PosTimestamp> period_poses;
        public MovmentCompensation(long _start_time,
                                   long _period,
                                   List<PosTimestamp> _period_poses, long _dt)
        {
            
            start_time = _start_time;
            period = _period;
            period_poses = _period_poses;
            dt = _dt;
        }
        static public MovmentCompensation comp_period(List<PosTimestamp> poses, double min_period = 1, double max_period = 6)//sec
        {
            var dt = 10;//10ms
            var poses_unif = uniform_time(poses,dt);
            var poses_smooth = PosTimestamp.line_aver(poses_unif, 100);//200 pt
            var min_period_ms = (long)(min_period * 1000);
            var max_period_ms = (long)(max_period * 1000);
            var max_dt = Math.Abs(poses_smooth[poses_smooth.Count - 1].time - poses_smooth[0].time);
            if (max_period_ms > max_dt * 0.4) max_period_ms = (long)(max_dt * 0.4);
            var st_ind = poses_smooth.Count / 2;
            var start_time = poses_smooth[st_ind].time;
            var period = find_periodic(poses_smooth, min_period_ms, max_period_ms, dt, st_ind);
            var period_int = period / dt;
            var period_poses = poses_smooth.GetRange(st_ind, (int)period_int);
            // Console.WriteLine(period);
            //foreach (PosTimestamp timestamp in poses_smooth) Console.WriteLine(timestamp.ToString());
            return new MovmentCompensation(start_time, period, period_poses,dt);
        }
        public PosTimestamp compute_cur_pos(long time)
        {
            var time_rel = (time - start_time) % period;
            var time_int = time_rel / period;
            return period_poses[(int)time_int];
        }
        static long find_periodic(List<PosTimestamp> poses,long min_period, long max_period,long dt, int st_ind)
        {
            if(dt == 0) return -1;
            var i_min = (int)((double)min_period / dt);
            var i_max = (int)((double)max_period / dt);            
            var d_pos = double.PositiveInfinity;
            var i_period = 0;
            for(int per_cur = i_min; per_cur <= i_max; per_cur++)
            {
                var ps1 = poses.GetRange(st_ind, per_cur);
                var ps2 = poses.GetRange(st_ind - per_cur, per_cur);
                var delt = PosTimestamp.arr_summ_abs(PosTimestamp.arr_minus(ps1, ps2));
                if(delt.pos1<d_pos)
                {
                    d_pos = delt.pos1;
                    i_period = per_cur;
                }
            }
            return i_period*dt;
        }


        static public List<PosTimestamp> uniform_time(List<PosTimestamp> poses,int dt = 10)
        {
            List<PosTimestamp> poses_unif = new List<PosTimestamp>();
            poses_unif.Add(poses[0]);
            var time_start = poses[0].time;
            var time_stop = poses[poses.Count-1].time;
            int k = 0;
            for (long time_cur = time_start; time_cur < time_stop && k < (poses.Count - 2); time_cur+=dt )
            {
                while (time_cur > poses[k+1].time) { k++; }
                poses_unif.Add(PosTimestamp.betw(poses[k], poses[k+1], time_cur));
                
            }

            return poses_unif;
        }


    }
}
