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
        LinearAxis linearAxis;

        public Scanner(CameraCV cam)
        {
            cameraCV = cam;
            laserSurface = new LaserSurface();
            pointCloud = new PointCloud();
            linearAxis = new LinearAxis();

        }

        public bool calibrateLaser(Mat[] mats,PatternType patternType,GraphicGL graphicGL = null)
        {
            return laserSurface.calibrate(mats, cameraCV, patternType, graphicGL);
        }

        public bool calibrateLinear(Mat[] mats, double[] positions, PatternType patternType, GraphicGL graphicGL = null)
        {
            return linearAxis.calibrate(mats, positions, cameraCV, patternType, graphicGL);
        }

        public bool addPoints(Mat mat)
        {
            return pointCloud.addPoints(mat, cameraCV, laserSurface);
        }
        public bool addPointsLin(Mat mat, double linPos)
        {
            return pointCloud.addPointsLin(mat, linPos,  cameraCV, laserSurface,linearAxis);
        }

        public bool addPointsLin(Mat[] mats, double[] linPos)
        {
            bool ret = false;
            if(mats.Length!=linPos.Length)
            {
                return false;
            }
            for(int i=0; i<mats.Length;i++)
            {
                ret = addPointsLin(mats[i],linPos[i]) ;
            }
            return ret;
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
