using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;

namespace opengl3
{
    public class Scanner
    {
        LaserSurface laserSurface;
        PointCloud pointCloud;
        CameraCV[] camerasCV;

        public Scanner(CameraCV[] cams)
        {
            init();
            camerasCV = cams;
        }

        void init()
        {

        }




    }
}
