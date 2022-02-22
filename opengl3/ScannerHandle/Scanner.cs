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
        CameraCV cameraCV;

        public Scanner(CameraCV cam)
        {
            cameraCV = cam;
            laserSurface = new LaserSurface();
            pointCloud = new PointCloud();
        }

        public bool calibrateLaser(Mat[] mats,PatternType patternType,GraphicGL graphicGL = null)
        {
            return laserSurface.calibrate(mats, cameraCV, patternType, graphicGL);
        }

        public bool addPoints(Mat mat)
        {

            return pointCloud.addPoints(mat, cameraCV, laserSurface);
        }

        public bool addPoints(Mat[] mats)
        {
            bool ret = false;
            foreach(var mat in mats)
            {
                ret = addPoints(mat);
            }
            return ret;
        }
        public Point3d_GL[] getPointsScene()
        {
            return pointCloud.points3d;
        }

        public Point3d_GL[] getPointsCam()
        {
            return Point3d_GL.multMatr(pointCloud.points3d,cameraCV.matrixSC);
        }
    }
}
