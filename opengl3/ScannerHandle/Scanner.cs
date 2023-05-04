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
        public PointCloud pointCloud;
        CameraCV cameraCV;
        StereoCamera stereoCamera;


        public LinearAxis linearAxis;

        public Scanner(CameraCV cam,LinearAxis linear=null)
        {
            cameraCV = cam;
            laserSurface = new LaserSurface();
            pointCloud = new PointCloud();
            if(linear==null)
            {
                linearAxis = new LinearAxis();
            }
            else
            {
                linearAxis = linear;
            }
            
            
        }
        public Scanner(CameraCV[] cameraCVs)
        {
            laserSurface = new LaserSurface();
            pointCloud = new PointCloud();
            linearAxis = new LinearAxis();
            stereoCamera = new StereoCamera(cameraCVs);

        }
        public Scanner(StereoCamera stereoCamera)
        {
            laserSurface = new LaserSurface();
            pointCloud = new PointCloud();
            linearAxis = new LinearAxis();
            this.stereoCamera = stereoCamera;

        }
        public void initStereo(Mat[] mats, PatternType patternType)
        {
            stereoCamera.calibrate(mats, patternType);
        }

        public Scanner set_coord_sys(StereoCamera.mode mode)
        {
            stereoCamera.scan_coord_sys = mode;
            return this;
        }
        public Scanner set_rob_pos(string pos)
        {
            stereoCamera.Bbf = new RobotFrame(pos).getMatrix();
            return this;
        }
        public void clearPoints()
        {
            pointCloud.clearPoints();
        }
        public bool calibrateLaser(Mat[] mats,PatternType patternType,GraphicGL graphicGL = null)
        {
            return laserSurface.calibrate(mats,null, cameraCV, patternType, graphicGL);
        }

        public bool calibrateLinear(Mat[] mats, double[] positions, PatternType patternType, GraphicGL graphicGL = null)
        {
            return linearAxis.calibrate(mats, positions, cameraCV, patternType, graphicGL);
        }

        public bool calibrateLinearStep(Mat[] mats,Mat orig, double[] positions, PatternType patternType, GraphicGL graphicGL = null)
        {
            return linearAxis.calibrateLas_step(mats, orig,positions, cameraCV, patternType,graphicGL);
        }
        
        public bool calibrateLinearLas(Mat[][] mats, Mat[] origs, double[] positions, PatternType patternType, GraphicGL graphicGL = null)
        {
            return linearAxis.calibrateLas(mats, origs, positions, cameraCV, patternType, graphicGL);
        }


        /// <summary>
        /// one line
        /// </summary>
        /// <param name="mats"></param>
        public void addPointsStereoLas(Mat[] mats, bool undist = true)
        {
            pointCloud.addPointsStereoLas(mats, stereoCamera, undist);
        }
        public void addPointsStereoLas_2d(Mat[] mats, bool undist = true)
        {
            pointCloud.addPoints2dStereoLas(mats, stereoCamera, undist);
        }

        public void addPointsSingLas_2d(Mat mat, bool undist = true, bool orig =false)
        {
            pointCloud.addPoints2dSingLas(mat, cameraCV, undist,orig);
        }

        public void compPointsStereoLas_2d()
        {
            pointCloud.comp_cross(stereoCamera);
        }
        /* public void addPointsStereoLas(Mat[][] mats)
         {
             for(int i=0;i<mats.Length;i++)
             {
                 Console.WriteLine("loading...      "+i+"/"+mats.Length);
                 pointCloud.addPointsStereoLas(mats[i], stereoCamera);
             }            
         }*/

        public bool addPoints(Mat mat)
        {
            return pointCloud.addPoints(mat, cameraCV, laserSurface);
        }
        public bool addPointsLin(Mat mat, double linPos)
        {
            return pointCloud.addPointsLin(mat, linPos,  cameraCV, laserSurface,linearAxis);
        }

        public bool addPointsLinLas(Mat mat, double linPos,Mat orig, PatternType patternType)
        {
            return pointCloud.addPointsLinLas(mat, linPos, cameraCV, linearAxis,orig, patternType);
        }

        public bool addPointsLinLas_step(Mat mat, Image<Bgr, byte> orig, double linPos, PatternType patternType)
        {
            return pointCloud.addPointsLinLas_step(mat,orig, linPos, cameraCV, linearAxis, patternType);
        }
        public int addPointsLinLas(Mat[] mats, double[] linPos,Mat orig, PatternType patternType)
        {
            int ret = 0;
            if (mats.Length != linPos.Length)
            {
                return 0;
            }
            for (int i = 0; i < mats.Length; i++)
            {
                if (addPointsLinLas(mats[i] - orig, linPos[i],orig,patternType))
                {
                    ret++;
                }
            }
            return ret;
        }
        public int addPointsLin(Mat[] mats, double[] linPos)
        {
            int ret = 0;
            if(mats.Length!=linPos.Length)
            {
                return 0;
            }
            for(int i=0; i<mats.Length;i++)
            {
                if(addPointsLin(mats[i],linPos[i]))
                {
                    ret++;
                }
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

        public Point3d_GL[][] getPointsLinesScene()
        {
            return pointCloud.points3d_lines.ToArray();
        }

        public Point3d_GL[] getPointsCam()
        {
            if(stereoCamera!=null)
            {
                return Point3d_GL.multMatr(pointCloud.points3d, stereoCamera.cameraCVs[0].matrixSC);
            }
            return Point3d_GL.multMatr(pointCloud.points3d,cameraCV.matrixSC);
        }
        public Point3d_GL[][] getPointsLinesCam()
        {
            return Point3d_GL.multMatr(pointCloud.points3d_lines.ToArray(), cameraCV.matrixSC);
        }


        public double[] enc_pos(string enc,int size)
        {
            var enc_pos = new double[size+10];
            enc = enc.Replace("\r", "");
            var lines = enc.Split('\n');
            foreach(var line in lines)
            {
                if (line.Length>1)
                {
                    var vals = line.Split(' ');
                    if(vals.Length==2)
                    {
                        var ind = try_int32(vals[1]);
                        var var = try_int32(vals[0]);
                        if(ind>0 && var>0)
                        {
                            enc_pos[ind] = var;
                        }
                    }
                    
                }
            }
            return enc_pos;
        }

        static int try_int32(string val)
        {
            try
            {
                return Convert.ToInt32(val);
            }
            catch(FormatException e)
            {
                return -1;
            }
            
        }
   
       
        
    
    }

}
