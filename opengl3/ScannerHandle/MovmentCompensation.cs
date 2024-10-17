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
        public double pos1{  get; set; }
        public double pos2{ get; set; }
        public double pos3 { get; set; }
        public long time { get; set; }
        public PosTimestamp(long _time,double _pos1, double _pos2 = 0, double _pos3 = 0)
        {
            pos3 = _pos3;
            pos2 = _pos2;
            pos1 = _pos1;
            time = _time;
        }

        public override string ToString()
        {
            return time + " " + pos1 + " " + pos2 + " " + pos3;
        }
    }
    class MovmentCompensation
    {


    }
}
