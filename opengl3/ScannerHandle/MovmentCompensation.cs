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
        double pos;
        long time;
        public PosTimestamp(double _pos, long _time)
        {
            pos = _pos;
            time = _time;
        }

        public override string ToString()
        {
            return time + " " + pos;
        }
    }
    class MovmentCompensation
    {


    }
}
