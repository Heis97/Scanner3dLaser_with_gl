using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;

namespace opengl3
{
    public class ScannerConfig
    {

        public int strip;
        [Description("Разреженность скана")]
        [Category("Настройки скана")]
        [DisplayName("strip")]
        public int Strip
        {
            get { return strip; }
            set { strip = value; }
        }

        public double smooth;
        [Description("Сглаживание, мм")]
        [Category("Настройки скана")]
        [DisplayName("smooth")]
        public double Smooth
        {
            get { return smooth; }
            set { smooth = value; }
        }
        public bool distort;
        [Description("Дисторсия")]
        [Category("Видео")]
        [DisplayName("distort")]
        public bool Distort
        {
            get { return distort; }
            set { distort = value; }
        }

        public bool load_3d;
        [Description("Загрузить 3д")]
        [Category("Видео")]
        [DisplayName("load_3d")]
        public bool Load_3d
        {
            get { return load_3d; }
            set { load_3d = value; }
        }

        public bool fast_load;
        [Description("Быстрая загрузка")]
        [Category("Видео")]
        [DisplayName("fast_load")]
        public bool Fast_load
        {
            get { return fast_load; }
            set { fast_load = value; }
        }

        public bool save_im;
        [Description("Сохранять изображения")]
        [Category("Видео")]
        [DisplayName("save_im")]
        public bool Save_im
        {
            get { return save_im; }
            set { save_im = value; }
        }

        public bool syncr;
        [Description("Синхронизировать изображения")]
        [Category("Видео")]
        [DisplayName("syncr")]
        public bool Syncr
        {
            get { return syncr; }
            set { syncr = value; }
        }

        public int buff_delt;
        [Description("Разница кадров")]
        [Category("Видео")]
        [DisplayName("buff_delt")]
        public int Buff_delt
        {
            get { return buff_delt; }
            set { buff_delt = value; }
        }

        public int las_offs;
        [Description("Отсеч кадров")]
        [Category("Видео")]
        [DisplayName("las_offs")]
        public int Las_offs
        {
            get { return las_offs; }
            set { las_offs = value; }
        }

        public int wind_regr;
        [Description("Окно регрессии")]
        [Category("Лазерная линия")]
        [DisplayName("wind_regr")]
        public int Wind_regr
        {
            get { return wind_regr; }
            set { wind_regr = value; }
        }

        public float board;
        [Description("Боковая рамка")]
        [Category("Лазерная линия")]
        [DisplayName("board")]
        public float Board
        {
            get { return board; }
            set { board = value; }
        }

        public bool reverse;
        [Description("Обратный поиск")]
        [Category("Лазерная линия")]
        [DisplayName("reverse")]
        public bool Reverse
        {
            get { return reverse; }
            set { reverse = value; }
        }

        public bool rotate;
        [Description("Поворот")]
        [Category("Лазерная линия")]
        [DisplayName("rotate")]
        public bool Rotate
        {
            get { return rotate; }
            set { rotate = value; }
        }

        public bool orig;
        [Description("Ориг")]
        [Category("Лазерная линия")]
        [DisplayName("orig")]
        public bool Orig
        {
            get { return orig; }
            set { orig = value; }
        }

        public int threshold;
        [Description("Минимальная яркость")]
        [Category("Лазерная линия")]
        [DisplayName("threshold")]
        public int Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        public int gauss_kern;
        [Description("Сглаживание")]
        [Category("Лазерная линия")]
        [DisplayName("gauss_kern")]
        public int Gauss_kern
        {
            get { return gauss_kern; }
            set { gauss_kern = value; }
        }
    }

    public class Scanner
    {
        LaserSurface laserSurface;
        public PointCloud pointCloud;
        public CameraCV cameraCV;
        public StereoCamera stereoCamera;


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
        static Matrix<double> comp_matr_calib(double r, double alp, double bet)// apl = z^z , bet = x^x
        {
            var z = r * Math.Cos(alp);
            var r_xy = r * Math.Sin(alp);
            var y = r_xy * Math.Cos(bet);
            var x = r_xy * Math.Sin(bet);
            var vz = new Point3d_GL(-x, -y, -z).normalize();
            var v_xy = new Point3d_GL(x, y, 0).normalize();
            var vx = Point3d_GL.vec_perpend_xy(v_xy);
            var vy = vz | vx;
            if (vy.z > 0)
            {
                vx = -vx;
                vy = vz | vx;
            }
            var matr = new Matrix<double>(new double[,] {
                { vx.x ,vx.y ,vx.z , x },
                { vy.x ,vy.y ,vy.z , y },
                { vz.x ,vz.y ,vz.z , z },
                { 0, 0, 0, 1},});
            return matr;
        }
        static RobotFrame comp_rob_fr_calib(double r, double alp, double bet)// apl = z^z , bet = x^x
        {
            var matr = comp_matr_calib(r, alp, bet);
            var fr = new RobotFrame(matr);
            return fr;
        }
        public void calibrate_scanner(MainScanningForm form)
        {

            var r = 100;// соизмеримо с фокусом камеры
            var alp_m = 0.4;
            var bet_m = Math.PI / 2;
            var angs = new double[,]
            {
                { 0 , 0 },
                { alp_m , 0 },
                { -alp_m , 0 },
                { alp_m , bet_m },
                { -alp_m , bet_m },
            };

            var ps_cam = new List<RobotFrame>();
            for (int i = 0; i < angs.Length; i++)
            {
                ps_cam.Add(comp_rob_fr_calib(r, alp_m, bet_m));
            }
            var ps_cam_n = new List<RobotFrame>();
            var ps_rob = new List<RobotFrame>();

            for (int j = 0; j < ps_cam.Count; j++)
            {
                RobotFrame p_rob, p_cam_n;
                (p_rob, p_cam_n) = go_to_pos_cam(ps_cam[j], form);
                ps_rob.Add(p_rob);
                ps_cam_n.Add(p_cam_n);
                go_to_pos_rob(ps_rob[0], form);
            }
        }
        public void go_to_pos_rob(RobotFrame frame_rob, MainScanningForm form)
        {
            form.send_rob_fr(frame_rob);
        }
        public (RobotFrame, RobotFrame) go_to_pos_cam(RobotFrame frame_cam, MainScanningForm form)// ret frame_rob, frame_cam end
        {
            // ...
            //
            var mat = form.get_im1();
            var ret = stereoCamera.cameraCVs[0].compPos(mat, PatternType.Mesh, new System.Drawing.Size(6, 7), 10, true);
            var ps = stereoCamera.cameraCVs[0].last_corners;
            var p_c_patt = new PointF((ps[21].X + ps[20].X) / 2, (ps[21].Y + ps[20].Y) / 2);
            var p_c = stereoCamera.cameraCVs[0].im_centr;
            //CvInvoke.CalibrateHandEye
            //get vec_xy


            return (null, null);
        }
        public void gen_next_p()
        {

        }

        public void initStereo(Mat[] mats, PatternType patternType,System.Drawing.Size pattern_size,float marksize)
        {
            stereoCamera.calibrate(mats, patternType, pattern_size,marksize);
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
            return linearAxis.calibrateLas_step(mats, orig,positions, cameraCV, patternType,graphicGL,pointCloud);
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
        public void addPointsStereoLas_2d(Mat[] mats, ScannerConfig config)
        {
            pointCloud.addPoints2dStereoLas(mats, stereoCamera, config);
        }

        public void addPointsStereoLas_2d_sync(Mat[] mats, double k, int cam_min, int cam_max, ScannerConfig config)
        {
            pointCloud.addPoints2dStereoLas_sync(mats, stereoCamera,k,cam_min,cam_max, config);
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


        
       
        
    
    }

}
