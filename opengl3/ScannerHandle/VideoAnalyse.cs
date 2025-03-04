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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace opengl3
{
    static class VideoAnalyse
    {
        static public Scanner loadVideo_stereo_not_sync(string filepath, Scanner scanner, ScannerConfig config, MainScanningForm form)
        {
            Console.WriteLine("not sync");
            var videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];
            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1,scanner.robotType);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }



            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var all_frames1 = capture1.Get(CapProp.FrameCount);
            var all_frames2 = capture2.Get(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            //form.get_combo_im().Items.Add(fr_st_vid);
            form.get_combo_im().BeginInvoke((MethodInvoker)(() => form.get_combo_im().Items.Add(fr_st_vid)));

            int buff_diff = config.buff_delt;
            int buff_len = buff_diff + 1;
            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                if(config.rotate_cam)
                {
                    CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                }
                
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = form.GL1;
            }
            var im1_buff = new Mat();
            var im2_buff = new Mat();

            var im1_buff_list = new List<Mat>();
            var im2_buff_list = new List<Mat>();

            //while (videoframe_count < 80)
            while (videoframe_count < all_frames - 1 - config.las_offs)
            {
                Mat im1 = new Mat();
                Mat im2 = new Mat();

                while (!capture1.Read(im1)) { }
                while (!capture2.Read(im2)) { }
                // Console.WriteLine(videoframe_count+"____________________");

                if (scanner != null && im1 != null && im2 != null)
                {
                    var buffer_mat1 = im1.Clone();
                    var buffer_mat2 = im2.Clone();
                    if (videoframe_count % config.strip == 0 && videoframe_count > buff_len)
                    {
                        // im1 -= orig1;
                        // im2 -= orig2;


                        /* CvInvoke.Imshow("im1_or", im1);
                         CvInvoke.Imshow("buffer_mat1", im1_buff_list[1]);
                         CvInvoke.Imshow("buffer_mat8", im1_buff_list[8]);
                       */

                        im1 -= im1_buff_list[buff_len - buff_diff];
                        im2 -= im2_buff_list[buff_len - buff_diff];
                        // CvInvoke.Imshow("im1_dif", im1);
                        // CvInvoke.WaitKey();
                        if (config.rotate_cam)
                        {
                            CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);
                        }
                        if (config.save_im)
                        {
                            var frame_d = new Frame(im1, im2, videoframe_count.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                            frames_show.Add(frame_d);
                        }
                        //scanner.addPointsStereoLas(new Mat[] { im1, im2 },false);
                        /*var ps1 = Detection.detectLineDiff(im1, 7);
                        var ps2 = Detection.detectLineDiff(im2, 7);

                        imageBox1.Image = UtilOpenCV.drawPointsF(im1, ps1, 255, 0, 0);
                        imageBox2.Image = UtilOpenCV.drawPointsF(im2, ps2, 255, 0, 0);*/
                        //CvInvoke.Imshow("im2", im2);                       
                        Console.WriteLine("add points");
                        scanner.addPointsStereoLas_2d(new Mat[] { im1, im2 }, config);

                    }

                    im1_buff = buffer_mat1.Clone();
                    im2_buff = buffer_mat2.Clone();

                    im1_buff_list.Add(im1_buff);
                    im2_buff_list.Add(im2_buff);
                    if (im1_buff_list.Count > buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                        im2_buff_list.RemoveAt(0);
                    }




                }
                videoframe_count++;
                Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            //form.get_combo_im().Items.AddRange(frames_show.ToArray());
            form.get_combo_im().BeginInvoke((MethodInvoker)(() => form.get_combo_im().Items.AddRange(frames_show.ToArray())));
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }
        static public Scanner loadVideo_stereo(string filepath, Scanner scanner, ScannerConfig config, MainScanningForm form)
        {
            Console.WriteLine("sync");
            var videoframe_count = 0;//var
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];

            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            string enc_path = ve_paths1[1];
            var pairs = frames_sync_from_file(enc_path, form.get_label_scan_res());
            var cam_min = (int)pairs[0][0];
            var cam_max = (int)pairs[0][1];
            var frame_min = (int)pairs[0][2];
            var frame_max = (int)pairs[0][3];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                
                scanner.set_coord_sys(StereoCamera.mode.world);
            }



            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var captures = new VideoCapture[] { capture1, capture2 };
            var all_frames1 = capture1.Get(CapProp.FrameCount);
            var all_frames2 = capture2.Get(CapProp.FrameCount);
            Console.WriteLine(all_frames1 + " " + all_frames2);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            //comboImages.Items.Add(fr_st_vid);

            int buff_diff = 5;
            int buff_len = buff_diff + 1;
            if (config.distort)
            {
                orig1 = scanner.stereoCamera.cameraCVs[0].undist(orig1);
                orig2 = scanner.stereoCamera.cameraCVs[1].undist(orig2);
            }
            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                if (config.rotate_cam)
                {
                    CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                }
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = form.GL1;
            }
            var im_min_buff_list = new List<Mat>();
            var im_max_buff_list = new List<Mat>();
            int f1 = 0;
            int f2 = 0;
            //f1 = 70;
            //while (f1 < frame_min-1)
            //int fr_c1 = 0;
            //int fr_c1 = 0;
            Console.WriteLine(cam_min + " " + cam_max);
            while (f1 < frame_min - 1 - config.las_offs)
            //while (f1 < 200)
            {
                im_min_buff_list = read_frame(captures[cam_min - 1], im_min_buff_list, buff_len); f1++;
                var f2_ind = (int)pairs[f1][0];
                var k = pairs[f1][1];
                while (f2 < f2_ind)
                //while (f2 != f2_ind)
                {
                    im_max_buff_list = read_frame(captures[cam_max - 1], im_max_buff_list, buff_len); f2++;
                }
                //Console.WriteLine(f1 + " " + f2);
                if (scanner != null)
                {
                    if (f1 % config.strip == 0 && f1 > buff_len)
                    {
                        var im_min = im_min_buff_list[buff_len - 1] - im_min_buff_list[buff_len - buff_diff];

                        var im_max = im_max_buff_list[buff_len - 1] - im_max_buff_list[buff_len - buff_diff];
                        var im_max_prev = im_max_buff_list[buff_len - 1 - 1] - im_max_buff_list[buff_len - buff_diff - 1];

                        //var im_min2 = im_min_buff_list[buff_len - 1];

                        //var im_max2 = im_max_buff_list[buff_len - 1];
                        if (config.rotate_cam)
                        {
                            if (cam_min == 2)
                            {
                                CvInvoke.Rotate(im_min, im_min, RotateFlags.Rotate180);

                                CvInvoke.Rotate(im_min, im_min, RotateFlags.Rotate180);

                            }
                            if (cam_max == 2)
                            {
                                CvInvoke.Rotate(im_max, im_max, RotateFlags.Rotate180);
                                CvInvoke.Rotate(im_max_prev, im_max_prev, RotateFlags.Rotate180);

                                // CvInvoke.Rotate(im_min2, im_min2, RotateFlags.Rotate180);
                                // CvInvoke.Rotate(im_max2, im_max2, RotateFlags.Rotate180);
                            }

                        }
                            
                        //Console.WriteLine(f1 + " " + f2);


                        //CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);


                        //if(videoframe_count!= 100 && videoframe_count != 103 && videoframe_count != 104 && videoframe_count != 145 && videoframe_count != 146 && videoframe_count <149 && videoframe_count > 100)
                        {

                            //var mat_c = scanner.stereoCamera.cameraCVs[cam_min - 1].undist(im_min);
                            var mats = Detection.detectLineDiff_ex(im_min, config);
                           
                            var frame_e = new Frame(mats[1].Clone(), mats[0].Clone(), "gauss_" + videoframe_count.ToString(), FrameType.LasDif);
                            
                            frame_e.stereo = true;
                            frames_show.Add(frame_e);
                            if (config.save_im)
                            {
                                var frame_d = new Frame(im_min.Clone(), im_max.Clone(), videoframe_count.ToString(), FrameType.LasDif);
                                frame_d.stereo = true;
                                frames_show.Add(frame_d);

                              //  frame_d = new Frame(im_min2.Clone(), im_max2.Clone(), videoframe_count.ToString(), FrameType.LasDif);
                               // frame_d.stereo = true;
                               // frames_show.Add(frame_d);
                            }

                            scanner.addPointsStereoLas_2d_sync(new Mat[] { im_min, im_max, im_max_prev }, k, cam_min, cam_max, config);
                        }

                    }
                }
                videoframe_count++;
                Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            form.get_combo_im().BeginInvoke((MethodInvoker)(() => form.get_combo_im().Items.AddRange(frames_show.ToArray())));
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }

        public static void photo_from_video(string filepath)
        {    
            var capture1 = new VideoCapture(filepath);
            var filename = Path.Combine( Path.GetPathRoot(filepath),Path.GetFileNameWithoutExtension(filepath));
            var all_frames1 = capture1.Get  (CapProp.FrameCount);
            var f1 = 0;
            while (f1 < all_frames1)
            {
                Mat im = new Mat();
                while (!capture1.Read(im)) { }
                if (f1%7==0)
                {
                    
                    im.Save(filepath + "_" + f1 + ".png");
                    
                }
                f1++;
            }
            
        }

        static void comp_glare(Mat mat)
        {
            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            CvInvoke.Threshold(mat, mat, 235, 255, ThresholdType.Binary);
           // CvInvoke.Imshow("glare", mat);
            //CvInvoke.WaitKey();
            
            var im = mat.ToImage<Gray, byte>();
            var i_nz = 0;
            for(int y = 0; y < im.Height; y++)
            {
                for (int x = 0; x < im.Width; x++)
                {
                    if (im.Data[y, x, 0] > 0)
                    {
                        i_nz++;
                        break;
                    }
                }
            }
            var nz = im.CountNonzero()[0];
            Console.WriteLine("NZ: "+i_nz+" "+im.Height+" "+nz + " " + (im.Size.Width * im.Size.Height));

        }

        static public Scanner loadVideo_sing_cam(string filepath, MainScanningForm form, Scanner scanner = null, ScannerConfig config = null, bool calib = false)
        {
            var videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            //var mat_or_tr = new Mat();
            //CvInvoke.AdaptiveThreshold(orig1, mat_or_tr,  255, ThresholdType.);

            //CvInvoke.Imshow("thr", mat_or_tr);
            
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            var ve_paths = get_video_path(1, filepath);
            string video_path = ve_paths[0];
            string enc_path = ve_paths[1];
            var name_v1 = Path.GetFileNameWithoutExtension(ve_paths[0]);
            scanner.set_rob_pos(name_v1,scanner.robotType);
            // scanner.stereoCamera.Bfs = 
            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.Get  (CapProp.FrameCount);
            // orig1 = scanner.cameraCV.undist(orig1);
            var fr_st_vid = new Frame(orig1, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            var pos_inc_cal = new List<double>();
            //comboImages.Items.Add(fr_st_vid);
            int buff_diff = config.Buff_delt;
            int buff_len = buff_diff + 1;
            var all_frames = all_frames1;
            if (scanner != null)
            {
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>() };
                scanner.pointCloud.graphicGL = form.GL1;
                //comp_glare(orig1);        
                    // scanner.cameraCV.matrixSC = new Matrix<double>(
                /*new double[,]
                {
                    { 0.9986744136, 0.0156131588, 0.0490473742, 25.2702716137 },
{ 0.013848945, -0.9992520102, 0.0361057723, 22.5256433504 },
{ 0.0495744124 ,- 0.0353786566 ,- 0.9981436411 ,125.1960146638},
        { 0 ,0, 0, 1} });
        }*/

                /*  new double[,]
                          {
                              {0.9984988882, 0.0187528173, 0.0514616562, 24.9030440612 },
      { 0.0167773194 ,- 0.9991156302, 0.0385548832, 22.0981226538},
      { 0.0521391577, - 0.0376336194, - 0.997930468, 123.1442586159},
                  { 0 ,0, 0, 1} });
              }*/
                /*  new double[,]
                          {
                              {0.969975639, -0.003123777 ,-0.2431820343, 45.7608462581  },
      { -0.0081239926, -0.9997756496, -0.0195614726, 26.8603275217},
      { -0.2430663706, 0.0209497609, -0.9697834021, 175.5494983004},
                  { 0 ,0, 0, 1} });
              }*/
            }
            var enc_file = "";
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc_file = sr.ReadToEnd();
            }
            //var inc_pos = enc_pos(enc_file, (int)all_frames);
            var enc_pos_time = analys_sync(enc_path);
            //enc_pos_time = recomp_pos_sing_linear(enc_pos_time);
            var inc_pos = enc_pos(enc_pos_time,false);

            

            var buffer_mat = new Mat();
            var im_orig = orig1.ToImage<Bgr, byte>();
            
            var im1_buff = new Mat();

            var im1_cals = new List<Mat>();
            var im1_buff_list = new List<Mat>();
            for(int i=0; i< inc_pos.Length; i++)
            {
               // Console.WriteLine(i+" "+inc_pos[i]);
            }
            //Console.WriteLine("start video_________");
            var p_match = new Point3d_GL();
            while (videoframe_count < all_frames - buff_len-5)//  "/2+1"   //-buff_len
            {
                Mat im1 = new Mat();
                while (!capture1.Read(im1)) { }
                if (scanner != null)
                {
                    var buffer_mat1 = im1.Clone();
                    //if (videoframe_count % strip == 0)
                    if ((videoframe_count % config.strip == 0 )&& (im1_buff_list.Count > buff_diff) && videoframe_count > 3)// && videoframe_count <83)
                    {

                        //var im1_or = im1.Clone();
                       // var ps_or = Detection.detectLineDiff(im1, config);
                        if(buff_diff>0)
                        {
                            im1 -= im1_buff_list[buff_len - buff_diff];

                        }
                        //var ps_diff = Detection.detectLineDiff(im1, config);

                        //p_match+=match_points(ps_or, ps_diff);

                        if (config.save_im) {
                            var frame_d = new Frame(im1.Clone(), im1.Clone(), videoframe_count.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                            frames_show.Add(frame_d);
                        }
                        // im1 = scanner.cameraCV.undist(im1);
                        /*//if (videoframe_count > 20)
                        {
                            var im1_or_un = scanner.cameraCV.undist(im1_or);
                            CvInvoke.Imshow("im1", im1_or_un);
                            CvInvoke.Imshow("buffer_mat", scanner.cameraCV.undist(im1_buff_list[buff_len - buff_diff]));
                            var im1_diff_un = scanner.cameraCV.undist(im1);
                            CvInvoke.Imshow("im1 diff", im1_diff_un);

                            var ps = Detection.detectLineDiff(im1_diff_un);
                            UtilOpenCV.drawPointsF(im1_or_un, ps, 0, 255, 0, 2);
                            CvInvoke.Imshow("im1-or_un", im1_or_un);
                            //CvInvoke.WaitKey();
                        }*/
                        //var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                        //frames_show.Add(frame_d);

                        /*  Console.Write(videoframe_count.ToString() + " " +
                              inc_pos[videoframe_count].ToString() + " " +
                              enc_pos_time[(videoframe_count - 1) * 2, 4] + " ");*/
                        // enc_pos_time[(videoframe_count-1)*2, 0]);

                        Console.Write(videoframe_count.ToString());
                        if (calib)
                        {
                            //var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                            // frames_show.Add(frame_d);

                           if( scanner.addPointsSingLas_2d(im1, true, calib,config));

                            {
                                pos_inc_cal.Add(inc_pos[videoframe_count]);
                                im1_cals.Add(im1);
                            }
                            
                        }
                        else scanner.addPointsLinLas_step(im1, im_orig, inc_pos[videoframe_count], PatternType.Mesh,config);

                    }
                    im1_buff = buffer_mat1.Clone();

                    im1_buff_list.Add(im1_buff);
                    if (im1_buff_list.Count > buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                    }
                }
                videoframe_count++;
               // Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            Console.WriteLine("p_match: "+p_match);
            //comboImages.Items.AddRange(frames_show.ToArray());
            Console.WriteLine("stop video_________");

            if (calib) scanner.calibrateLinearStep(im1_cals.ToArray(), im_orig.Mat, pos_inc_cal.ToArray(), PatternType.Mesh, form.GL1, config);

            //var mats = Frame.getMats(frames_show.ToArray());
            //var corn = Detection.detectLineDiff_corn_calibr(mats);

            //UtilOpenCV.drawPointsF(orig1, corn, 255, 0, 0, 2, true);
            //CvInvoke.Imshow("corn", orig1);
            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            return scanner;
        }
        static public Scanner loadVideo_sing_cam_move(string filepath, MainScanningForm form, Scanner scanner = null, ScannerConfig config = null, bool calib = false)
        {
            var videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            var ve_paths = get_video_path(1, filepath);
            string video_path = ve_paths[0];
            string enc_path = ve_paths[1];

            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.Get(CapProp.FrameCount);
            // orig1 = scanner.cameraCV.undist(orig1);
            var fr_st_vid = new Frame(orig1.Clone(), "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            var pos_inc_cal = new List<double>();
            //comboImages.Items.Add(fr_st_vid);
            int buff_diff = config.Buff_delt;
            int buff_len = buff_diff + 1;
            var all_frames = all_frames1;
            if (scanner != null)
            {
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.Clone().ToImage<Bgr, byte>() };
                scanner.pointCloud.graphicGL = form.GL1;
               // comp_glare(orig1);
            }
            var enc_file = "";
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc_file = sr.ReadToEnd();
            }
            //var inc_pos = enc_pos(enc_file, (int)all_frames);
            var enc_pos_time = analys_sync(enc_path);
            //enc_pos_time = recomp_pos_sing_linear(enc_pos_time);
            var inc_pos = enc_pos(enc_pos_time, false);



            var buffer_mat = new Mat();
            var im_orig = orig1.Clone().ToImage<Bgr, byte>();

            var im1_buff = new Mat();

            var im1_cals = new List<Mat>();
            var im1_buff_list = new List<Mat>();



            var ims1 = new List<Mat>();
            
            while (videoframe_count < all_frames)
            {

                Mat im1 = new Mat();
                while (!capture1.Read(im1)) { }
                ims1.Add(im1);

                videoframe_count++;
                Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }

            var p_match1 = new Point3d_GL();
            var p_match2 = new Point3d_GL();
            var len = ims1.Count-config.las_offs;
            var ims1_diff = diff_mats_bf(ims1.ToArray(), 20,len,config.strip);
            
            for (int i = 0; i < len; i++)
            {
                if (scanner != null && ims1_diff[i] != null)
                {
                    if (i % config.strip == 0)
                    {

                        var im1_or = ims1[i].Clone()- orig1.Clone();
                        var ps_cur = Detection.detectLineDiff(ims1[i].Clone(), config);
                        var ps_or = Detection.detectLineDiff(im1_or.Clone(), config);
                        var ps_diff = Detection.detectLineDiff(ims1_diff[i].Clone(), config);

                       /* if(i==15)
                        {
                            var m_or = orig1.Clone();
                            var m_diff_or = im1_or.Clone();
                            var m_cur = ims1[i].Clone();
                            var m_diff_opt = ims1_diff[i].Clone();
                            var j_min = 235;
                            var m_diff_opt_sec = ims1[j_min].Clone();
                            var y = 403;
                            var pixs = new List<int[]>();

                            CvInvoke.GaussianBlur(m_diff_or, m_diff_or, new Size(7, 7), 3);
                            CvInvoke.GaussianBlur(m_diff_opt, m_diff_opt, new Size(7, 7), 3);
                            CvInvoke.GaussianBlur(m_cur, m_cur, new Size(7, 7), 3);
                            //UtilOpenCV.drawLines(m_diff_or, new System.Drawing.PointF[] { new System.Drawing.PointF(0, y), new System.Drawing.PointF(m_diff_or.Width - 1, y) }, 0, 0, 255);
                            pixs.Add(UtilOpenCV.takeLineFromMat(m_or, y));
                            pixs.Add(UtilOpenCV.takeLineFromMat(m_diff_or, y));
                            pixs.Add(UtilOpenCV.takeLineFromMat(m_diff_opt, y));
                            pixs.Add(UtilOpenCV.takeLineFromMat(m_cur, y));
                            pixs.Add(UtilOpenCV.takeLineFromMat(m_diff_opt_sec, y));

                           
                            Console.WriteLine("m_or m_diff_or m_diff_opt m_cur m_diff_opt_sec");
                            for(int k=0; k < pixs[0].Length;k++)
                            {
                                for (int w = 0; w < pixs.Count; w++)
                                {
                                    Console.Write(pixs[w][k] + " ");
                                }
                                Console.WriteLine();
                            }

                            UtilOpenCV.drawLines(m_diff_or, new System.Drawing.PointF[] { new System.Drawing.PointF(0, y), new System.Drawing.PointF(m_diff_or.Width - 1, y) }, 255, 255, 255,2);
                            UtilOpenCV.drawLines(m_diff_opt, new System.Drawing.PointF[] { new System.Drawing.PointF(0, y), new System.Drawing.PointF(m_diff_or.Width - 1, y) }, 255, 255, 255,2);
                            UtilOpenCV.drawLines(m_cur, new System.Drawing.PointF[] { new System.Drawing.PointF(0, y), new System.Drawing.PointF(m_diff_or.Width - 1, y) }, 255, 255, 255,2);

                            m_diff_or =  UtilOpenCV.drawPointsF(m_diff_or, ps_or, 0, 255, 0,2);
                            m_diff_opt = UtilOpenCV.drawPointsF(m_diff_opt, ps_diff, 0, 255, 0,2);
                            m_cur =   UtilOpenCV.drawPointsF(m_cur, ps_cur, 0, 255,0,2);
                            var roi = new Rectangle(0, y-100, m_diff_or.Width - 1, 200);
                            m_diff_or = new Mat(m_diff_or, roi);
                            m_diff_opt = new Mat(m_diff_opt, roi);
                            m_cur = new Mat(ims1[i].Clone(), roi);

                            m_or = new Mat(m_or, roi);
                            m_diff_opt_sec = new Mat(m_diff_opt_sec, roi);


                           CvInvoke.Imshow("m_or :", m_or);
                            CvInvoke.Imshow("m_diff_or :", m_diff_or);
                            CvInvoke.Imshow("m_diff_opt :", m_diff_opt);
                            CvInvoke.Imshow("m_cur :", m_cur);
                            CvInvoke.Imshow("m_diff_opt_sec  :", m_diff_opt_sec);
                            Console.Write(i + " ");
                            UtilOpenCV.drawPointsF(im1_or, ps_or, 255, 0, 0);
                            UtilOpenCV.drawPointsF(im1_or, ps_diff, 0, 255, 0);
                            UtilOpenCV.drawPointsF(im1_or, ps_cur, 100, 100, 255);
                            CvInvoke.Imshow("ps_or:", im1_or);
                            CvInvoke.WaitKey();
                        }                       */
                        p_match1+=match_points(ps_or, ps_diff);
                        p_match2 += match_points(ps_cur, ps_diff);
                        if (config.save_im)
                        {
                            var frame_d = new Frame(ims1_diff[i].Clone(), ims1_diff[i].Clone(), i.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                            frames_show.Add(frame_d);
                        }
                        ims1_diff[i] = ims1[i].Clone();
                        scanner.addPointsLinLas_step(ims1_diff[i].Clone(), im_orig, inc_pos[i], PatternType.Mesh, config);
                        GC.Collect();
                    }
                }

            }




            /*

            while (videoframe_count < all_frames - buff_len - 5)//  "/2+1"   //-buff_len
            {
                Mat im1 = new Mat();
                while (!capture1.Read(im1)) { }
                if (scanner != null)
                {
                    var buffer_mat1 = im1.Clone();
                    //if (videoframe_count % strip == 0)
                    if ((videoframe_count % config.strip == 0) && (im1_buff_list.Count > buff_diff) && videoframe_count > 30)// && videoframe_count <83)
                    {

                   
                         scanner.addPointsLinLas_step(im1, im_orig, inc_pos[videoframe_count], PatternType.Mesh, config);

                    }
                    im1_buff = buffer_mat1.Clone();
                    im1_buff_list.Add(im1_buff);
                    if (im1_buff_list.Count > buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                    }
                }
                videoframe_count++;
                // Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }*/
            Console.WriteLine("p_match_or: " + p_match1+ "p_match_curr: " + p_match2);
            Console.WriteLine("stop video_________");
            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            return scanner;
        }
        static Point3d_GL match_points(PointF[] ps_or, PointF[] ps_diff)
        {
            var i_all = 0;
            var i_match = 0;
            for(int i = 0; i < 700; i++)
            {
                var i_or =  LaserSurface.ind_y(ps_or, i);
                var i_diff = LaserSurface.ind_y(ps_diff, i);
                if(i_or>0 && i_diff>0)
                {
                    var dx = ps_or[i_or].X - ps_diff[i_diff].X;
                    i_all++;
                    if(Math.Abs(dx)<3) i_match++;
                }
            }
            Console.WriteLine(i_all + " " + i_match);
            return new Point3d_GL(i_all,i_match);
        }
        static public void loadVideo_test_laser(string filepath)
        {

            ImageViewer viewer = new ImageViewer(); //create an image viewer
            viewer.SetBounds(0, 0, 1620, 1080);
            VideoCapture capture = new VideoCapture(filepath); //create a camera captue
            var mat_f = new Mat();
            bool first = true;
            Application.Idle += new EventHandler(delegate (object sender, EventArgs e)
            {  //run this until application closed (close button click on image viewer)
                var mat = capture.QueryFrame();
                if (first) { mat_f = mat.Clone(); first = false; }

                var pf = Detection.detectLineSensor(mat)[0];
                var p = new System.Drawing.Point((int)pf.X, (int)pf.Y);
                Console.WriteLine(pf.X);
                CvInvoke.DrawMarker(mat, p, new MCvScalar(255, 0, 0), MarkerTypes.TiltedCross, 10);
                viewer.Image = mat; //draw the image obtained from camera

            });
            viewer.ShowDialog();


            /*string video_path = filepath;
            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            Console.WriteLine(all_frames1);
            while (capture1.IsOpened)
            {
                Mat im1 = new Mat();
                var ok = capture1.Read(im1);
                if(ok)  CvInvoke.Imshow("v1", im1);
                
               
               // videoframe_count++;
                //Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames1);
            }*/
        }
        static List<Mat> read_frame(VideoCapture capture, List<Mat> buff, int len)
        {
            Mat im = new Mat();
            while (!capture.Read(im)) { }
            buff.Add(im);
            if (buff.Count > len) buff.RemoveAt(0);
            return buff;
        }


        static Mat[] load_video(string path)
        {

            var capture = new VideoCapture(path);
            var all_frames = capture.Get(CapProp.FrameCount);
            var frames = new List<Mat>();
            var videoframe_count = 0;

            //CvInvoke.mea
            while (videoframe_count < all_frames)
            {
                Mat im = new Mat();
                while (!capture.Read(im)) { }
                frames.Add(im);
                videoframe_count++;
            }
            Console.WriteLine(path + " loaded.");
            return frames.ToArray();
        }

        public static void noise_analyse(string path)
        {
            var mats = load_video(path);
            var datas = new List<byte[,,]>();
            for(int i=0; i < mats.Length; i++)
            {
                var data = (byte[,,])mats[i].GetData();
                datas.Add(data);
            }
          /*  var data_an_r = new List<double>();
            var data_an_g = new List<double>();
            var data_an_b = new List<double>();

            var data_an = new List<MCvScalar>();

            
            for (int i = 0; i < datas.Count;i++)
            {
                data_an_r.Add(datas[i][0, 0, 0]);
                data_an_g.Add(datas[i][0, 0, 1]);
                data_an_b.Add(datas[i][0, 0, 2]);

                Console.WriteLine(datas[i][0, 0, 0] + " " + datas[i][0, 0, 1] + " " + datas[i][0, 0, 2] + " ");
                data_an.Add(new MCvScalar(datas[i][0, 0, 0],datas[i][0, 0, 1],datas[i][0, 0, 2]));
            }

            var ps = UtilOpenCV.Mcvscalar_to_mat(data_an.ToArray());
            MCvScalar mean = new MCvScalar(0, 0, 0);
            MCvScalar std_dev = new MCvScalar(0, 0, 0);
            CvInvoke.MeanStdDev(ps,ref mean, ref std_dev);
            Console.WriteLine(mean.V0 + " " + std_dev.V0);
            Console.WriteLine(mean.V1 + " " + std_dev.V1);
            Console.WriteLine(mean.V2 + " " + std_dev.V2);*/


            var w = datas[0].GetLength(0);
            var h = datas[0].GetLength(1);
            var im = new Image<Bgr, byte>(w, h);
            var x_sh = 0;
            var data_pix = new double[datas.Count, 3];
            int dxy = 2;
            for (int x = 0; x < w; x+= dxy)
            {
               
                var y_sh = 0;
                for (int y = 0; y < h; y+= dxy)
                {
                    
                    

                    for (int i = 0; i < datas.Count; i++)
                    {
                        data_pix[i, 0] = datas[i][x, y, 0];
                        data_pix[i, 1] = datas[i][x, y, 1];
                        data_pix[i, 2] = datas[i][x, y, 2];
                        //Console.WriteLine(data_pix_r[i].V0);
                    }
                    
                    MCvScalar mean = new MCvScalar();
                    MCvScalar std_dev = new MCvScalar();
                    (mean, std_dev) = UtilOpenCV.std_dev_3ch(data_pix);

                    im.Data[y_sh, x_sh, 0] = (byte)std_dev.V0;
                    im.Data[y_sh, x_sh, 1] = (byte)std_dev.V1;
                    im.Data[y_sh, x_sh, 2] = (byte)std_dev.V2;

                    //Console.WriteLine(std_dev.V0 + " " + std_dev.V1 + " " + std_dev.V2);
                    y_sh++;
                }
                GC.Collect();
                Console.WriteLine(x + "/" + w);
                x_sh++;
            }
              im = im * 30;
              var mat_rgb = im.Mat.Split();
              CvInvoke.Imshow("b", mat_rgb[0]);
              CvInvoke.Imshow("g", mat_rgb[1]);
              CvInvoke.Imshow("r", mat_rgb[2]);
            
        }

        static public double[] enc_pos(string enc, int size)
        {
            var enc_pos = new double[size + 10];
            enc = enc.Replace("\r", "");
            var lines = enc.Split('\n');
            foreach (var line in lines)
            {
                if (line.Length > 1)
                {
                    var vals = line.Split(' ');
                    //if(vals.Length==2)
                    {
                        var ind = try_int32(vals[2]);
                        var var = try_int32(vals[0]);
                        if (ind > 0 && var > 0)
                        {
                            enc_pos[ind] = var;
                        }
                    }

                }
            }
            return enc_pos;
        }

        static public double[] enc_pos(double[,] enc,bool pos = true)
        {
            var enc_pos = new double[enc.GetLength(0)];
            for (int i = 0; i < enc.GetLength(0); i += 2)
            {
                var ind = (int)enc[i, 2];
                var var = enc[i, 0];
                if (!pos) { var = ind; }
                if (ind > 0)
                {
                    enc_pos[ind] = var;
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
            catch (FormatException e)
            {
                return -1;
            }

        }


        static string[] get_video_path(int ind, string filepath)//[ video, enc]
        {
            var files = Directory.GetFiles("cam" + ind + "\\" + filepath);
            string video_path = "";
            string enc_path = "";
            foreach (var path in files)
            {
                var ext = path.Split('.').Reverse().ToArray();

                if (ext[0] == "txt")
                {
                    enc_path = path;
                }
                if (ext[0] == "mp4")
                {
                    video_path = path;
                }
            }
            return new string[] { video_path, enc_path };
        }

        static int[] frames_max(double[,] data)
        {
            int analyse_len = 40;
            var end_data = new List<int[]>();
            for (int i = data.GetLength(0) - analyse_len; i < data.GetLength(0) - 1; i++)
            {
                end_data.Add(new int[] {(int) data[i, 2],(int) data[i, 3] });
            }
            var ed_s = (from d in end_data
                        orderby d[0] descending
                        select d).ToArray();
            var i_min = 0;
            for (int i = 1; i_min == 0 && i < analyse_len - 2; i++)
            {
                if (ed_s[i][1] != ed_s[0][1]) i_min = i;
            }
            var ed_l = ed_s[0].ToList();
            ed_l.AddRange(ed_s[i_min]);
            return ed_l.ToArray();
        }
        static public double[][] frames_sync_from_file(string enc_path, Label label = null)
        {
            var data = analys_sync(enc_path);
            var frms_max = frames_max(data);

            var fr_max = frms_max[0];
            var cam_max = frms_max[1];
            var fr_min = frms_max[2];
            var cam_min = frms_max[3];


           // Console.WriteLine("fr_cnt = " + fr_max);
           // Console.WriteLine("fr_cnt_m = " + fr_min);
            var data_s = new int[3, fr_max + 1][];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                var fr_n =(int) data[i, 2];
                var cam_n = (int)data[i, 3];
                var time = (int)data[i, 4];
                if (data_s[cam_n, fr_n] == null)
                {
                    data_s[cam_n, fr_n] = new int[0];
                }
                var l = data_s[cam_n, fr_n].ToList();
                l.Add(time);
                data_s[cam_n, fr_n] = l.ToArray();
            }

            var find_prec1 = new List<double>();
            var find_prec2 = new List<double>();

            for (int i = 1; i < data_s.GetLength(1); i++)
            {
                if (data_s[cam_max, i] != null && data_s[cam_min, i] != null && data_s[cam_max, i - 1] != null && data_s[cam_min, i - 1] != null)
                    if (data_s[cam_max, i].Length > 1 && data_s[cam_min, i].Length > 1 && data_s[cam_max, i - 1].Length > 1 && data_s[cam_min, i - 1].Length > 1)
                    {
                       /* Console.WriteLine(i + " " + data_s[cam_max, i][0] + "  " + data_s[cam_min, i][0] + " " + //текущее время
                            (data_s[cam_max, i][1] - data_s[cam_max, i][0]) + "  " + (data_s[cam_min, i][1] - data_s[cam_min, i][0]) + " " +//дельта между началом кадра и концом
                            (data_s[cam_max, i][0] - data_s[cam_max, i - 1][0]) + " " + (data_s[cam_min, i][0] - data_s[cam_min, i - 1][0]) + " " +//дельта между началами соседних кадров
                            (data_s[cam_max, i][1] - data_s[cam_max, i - 1][1]) + " " + (data_s[cam_min, i][1] - data_s[cam_min, i - 1][1]) + " ");//дельта между концами соседних кадров*/
                        find_prec1.Add(data_s[cam_max, i][0] - data_s[cam_max, i - 1][0]);
                        find_prec2.Add(data_s[cam_min, i][0] - data_s[cam_min, i - 1][0]);
                    }
            }
            var prec1 = find_aver_dev(find_prec1.ToArray());
            var prec2 = find_aver_dev(find_prec2.ToArray());
            Console.WriteLine(Math.Round(prec1).ToString() + " " + Math.Round(prec2).ToString() + " PREC");
            if (label != null) label.Text = Math.Round(prec1).ToString() + " " + Math.Round(prec2).ToString() + " PREC";

            var prs = compare_frames(data_s, fr_min, fr_max, cam_min, cam_max);
            return prs;
        }
       
        static double find_aver_dev(double[] vals)
        {
            double len = vals.Length;
            var aver = vals.Sum() / len;
            var sq_arr = new double[vals.Length];
            for (int i = 0; i < sq_arr.Length; i++)
            {
                sq_arr[i] = (vals[i] - aver) * (vals[i] - aver);
            }
            var sq_aver = sq_arr.Sum() / len;
            return Math.Sqrt(sq_aver);
        }

        static double[][] compare_frames(int[,][] data, int frame_min, int frame_max, int cam_min, int cam_max)
        {
            int j = 1;
            var pairs = new double[frame_min][];
            for (int i = 1; i < frame_min; i++)
            {
                if (data[cam_min, i] != null && data[cam_max, j] != null)
                {
                    while (data[cam_min, i][0] > data[cam_max, j][0] && j < frame_max)
                    {
                        j++;
                    }
                    double df = 0;//на какую часть нужно сместиться относительно j-1 кадра чтобы совпасть по времени
                    if (j > 1)
                    {
                        df = (double)(data[cam_max, i][0] - data[cam_max, j - 1][0]) / (data[cam_max, j][0] - data[cam_max, j - 1][0]);

                        var d1 = data[cam_max, i][0];
                        var d2 = data[cam_max, j][0] - (data[cam_max, j][0] - data[cam_max, j - 1][0]) * (1 - df);
                        //Console.WriteLine(d1 + " " + d2);
                    }
                    pairs[i] = new double[] { j, df };

                }
                //Console.WriteLine(i + " " + j);
            }
            pairs[0] = new double[] { cam_min, cam_max, frame_min, frame_max };
            return pairs;
        }
        public static double[,] analys_sync(string enc_path)
        {
            string enc;
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc = sr.ReadToEnd();
            }

            enc = enc.Replace("\r", "");
            var lines = enc.Split('\n');
            var enc_pos = new double[lines.Length, 8];
            int ind = 0;
            int st_time = 0;
            foreach (var line in lines)
            //for (int l = lines.Length-1; l>=0; l--)
            {
                //var line = lines[l];
                if (line.Length > 0)
                {
                    var vals = line.Trim().Split(' ');

                   //if (vals.Length == 6)
                    {
                        for (int i = 0; i < vals.Length; i++)
                        //for (int i = vals.Length - 1; i >= 0; i--)
                        {
                            if (vals[i].Length>8)
                            {
                                string time = "";
                                for (int j = vals[i].Length / 2; j < vals[i].Length; j++)
                                {
                                    time += vals[i][j];
                                }
                                vals[i] = time;
                                enc_pos[ind, i] = Convert.ToInt32(vals[i]);
                                enc_pos[ind, i] -= st_time;
                                if (ind == 0)
                                {
                                    st_time = (int)enc_pos[ind, i];
                                    enc_pos[ind, i] = 0;
                                }
                            }
                            else if(vals[i].Length ==0)
                            {
                                enc_pos[ind, i] = 0;
                            }
                            else
                            {
                                enc_pos[ind, i] = Convert.ToInt32(vals[i]);
                            }
                            

                        }
                        //Console.Write(vals[i] + ";");
                       /* if (enc_pos[ind, 0] == 0) Console.WriteLine(enc_pos[ind, 3]+" ");
                        if (enc_pos[ind, 0] != 0) Console.Write(enc_pos[ind, 3]+" ");
                        if (enc_pos[ind, 5] == 1) Console.Write(enc_pos[ind, 4] + ";" + ";");
                         if (enc_pos[ind, 5] == 2) Console.Write(";" + enc_pos[ind, 4] + ";");*/


                        // Console.WriteLine();
                    }
                }
                ind++;
            }
            return enc_pos;
        }

        static double[,] recomp_pos_sing_time(double[,] data)
        {
            var len = data.GetLength(0)/2;
            if (len % 2 != 0) len -= 1;
            int f_ind = 6;
            int l_ind = len;
            double pos_f = data[f_ind, 0];
            double pos_l = data[l_ind, 0];

            double time_f = data[f_ind, 3];
            double time_l = data[l_ind, 3];
            var data_c = (double[,])data.Clone();


            for (int i=f_ind;i<len;i+=2)
            {
                var time = data[i+1, 3];
                var pos = data[i, 0];
                var pos_re = ((time - time_f)/(time_l-time_f) )*(pos_l-pos_f)+pos_f;
                data_c[i, 0] = pos_re;
            }
            return data_c;
        }
        public static double[,] recomp_pos_sing_linear(double[,] data)
        {
            var len = data.GetLength(0) ;

            if (len % 2 != 0) len -= 1;
            int f_ind = 6;
            int l_ind = len;
            double pos_f = data[f_ind, 0];
            double pos_l = data[l_ind, 0];
            double pos_m = data[l_ind/2, 0];
            //double time_f = data[f_ind, 3];
            //double time_l = data[l_ind, 3];
            var data_c = (double[,])data.Clone();
            var regr_pos = new List<double[]>();
            var regr_time = new List<double[]>();
            for (int i= f_ind+30 ;i<l_ind;i+=2)
            {
                if (data[i, 0] != 0)
                {
                    /*regr_pos.Add(new double[] { data[i, 2], data[i, 0] });
                    regr_time.Add(new double[] { data[i, 2], data[i, 4] });
                    Console.WriteLine(data[i, 2] + " " + data[i, 1] + " " + data[i, 0]);*/

                    regr_pos.Add(new double[] { data[i, 2], data[i, 0] });
                    regr_time.Add(new double[] { data[i, 1], data[i, 2] });
                    //Console.WriteLine(data[i, 2] + " " + data[i, 1] + " " + data[i, 0]);

                    Console.WriteLine(data[i, 2] + " " + data[i, 1] + " " + data[i, 0] + " " + data[i, 4]);
                }
                
            }

            var pos_koef = Regression.regression(regr_pos.ToArray(),1);
            var time_koef = Regression.regression(regr_time.ToArray(), 1);

            //var dd = (pos_l - pos_m) / (l_ind / 2);
            for (int i = 0; i < len; i ++)
            {
                //var time = data[i + 1, 3];
                //var pos = data[i, 0];
                //var pos_re = ((time - time_f) / (time_l - time_f)) * (pos_l - pos_f) + pos_f;

                //data_c[i, 0] = Regression.calcPolynSolv(pos_koef, data[i, 2]);
                //data_c[i, 0] = data_c[i, 2];
            }
            Console.WriteLine("time_delt");
            for (int i = f_ind; i < l_ind; i += 2)//f_ind-1
            {
                if (data[i, 0] != 0)
                {
                    var time_del = Regression.calcPolynSolv(time_koef, data[i, 1]) - data[i, 2];
                    Console.WriteLine(data_c[i, 0] + " " + data_c[i, 1] + " " + time_del);
                }
                    
            }

            return data_c;
        }
        static Mat get_frame_video(string video_path, int frame)
        {
            var capture = new VideoCapture(video_path);
            var mat = new Mat();
            capture.Set(CapProp.PosFrames, frame);
            capture.Retrieve(mat);
            capture.Dispose();
            return mat;
        }
        public static Scanner video_delt(string filepath, Scanner scanner, ScannerConfig config, MainScanningForm form, int ref_fr = 0)
        {
            var videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];
            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }
            int ref_frame = ref_fr;
            orig1 = get_frame_video(video_path1, ref_frame);
            orig2 = get_frame_video(video_path2, ref_frame);

            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var all_frames1 = capture1.Get(CapProp.FrameCount);
            var all_frames2 = capture2.Get(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            form.get_combo_im().Items.Add(fr_st_vid);

            

            int buff_diff = config.buff_delt;
            int buff_len = buff_diff + 1;
            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = form.GL1;
            }
            var im1_buff = new Mat();
            var im2_buff = new Mat();

            var im1_buff_list = new List<Mat>();
            var im2_buff_list = new List<Mat>();

            var features = new Features();

            //while (videoframe_count < 80)
            while (videoframe_count < all_frames - config.las_offs)
            {
                
                Mat im1 = new Mat();
                Mat im2 = new Mat();

                while (!capture1.Read(im1)) { }
                while (!capture2.Read(im2)) { }
                // Console.WriteLine(videoframe_count+"____________________");
                if (videoframe_count == 0)
                {
                   // orig1 = im1.Clone();
                  //  orig2 = im2.Clone();
                }
                if (scanner != null && im1 != null && im2 != null)
                {
                    var buffer_mat1 = im1.Clone();
                    var buffer_mat2 = im2.Clone();
                    if (videoframe_count % config.strip == 0 && videoframe_count > buff_len)
                    {

                        im1 = orig1 - im1;
                        im2 = orig2 - im2;
                        var dev1 = deviation_light_gauss(im1);
                        var dev2 = deviation_light_gauss(im2);
                        Console.WriteLine(videoframe_count + " "+dev1+" "+dev2);

                        CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);
                        if (config.save_im)
                        {
                            var frame_d = new Frame(im1, im2, videoframe_count.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                           // im2 = im1_buff_list[buff_len - buff_diff].Clone();
                            //frame_d.im_dif = features.drawDescriptorsMatch(ref im1, ref im2);
                            
                            frames_show.Add(frame_d);
                        }
                    }

                    im1_buff = buffer_mat1.Clone();
                    im2_buff = buffer_mat2.Clone();

                    im1_buff_list.Add(im1_buff);
                    im2_buff_list.Add(im2_buff);
                    if (im1_buff_list.Count > buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                        im2_buff_list.RemoveAt(0);
                    }
                }
                videoframe_count++;
                //Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            //scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }
        public static Scanner video_delt_bf(string filepath, Scanner scanner, ScannerConfig config, MainScanningForm form)
        {
            var videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];
            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }
            int ref_frame = 20;
            orig1 = get_frame_video(video_path1, ref_frame);
            orig2 = get_frame_video(video_path2, ref_frame);

            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var all_frames1 = capture1.Get(CapProp.FrameCount);
            var all_frames2 = capture2.Get(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            form.get_combo_im().Items.Add(fr_st_vid);

            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = form.GL1;
            }

            var ims1 = new List<Mat>();
            var ims2 = new List<Mat>();
            while (videoframe_count < all_frames - config.las_offs)
            {

                Mat im1 = new Mat();
                Mat im2 = new Mat();

                while (!capture1.Read(im1)) { }
                while (!capture2.Read(im2)) { }
                ims1.Add(im1);
                ims2.Add(im2);
                
                videoframe_count++;
                Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }

            var ims1_diff = diff_mats_bf(ims1.ToArray(), config.buff_delt);
            var ims2_diff = diff_mats_bf(ims2.ToArray(), config.buff_delt);


            var len = Math.Min(ims1_diff.Length, ims2_diff.Length);
            for(int i=0; i<len; i++)
            {
                if (scanner != null && ims1_diff[i] != null && ims2_diff[i] != null)
                {
                    if (i % config.strip == 0 )
                    {

                        CvInvoke.Rotate(ims2_diff[i], ims2_diff[i], RotateFlags.Rotate180);
                        if (config.save_im)
                        {
                            var frame_d = new Frame(ims1_diff[i], ims2_diff[i], i.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                            frames_show.Add(frame_d);
                        }
                        scanner.addPointsStereoLas_2d(new Mat[] { ims1_diff[i], ims2_diff[i] }, config);
                        GC.Collect();
                    }
                }

            }

            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }

        static Mat[] diff_mats_bf(Mat[] mats, int wind, int len = -1, int strip = -1, int wind_max = 300)
        {
            var mats_diff = new Mat[mats.Length];
            var mats_len = mats.Length;
            if (len > 0) mats_len = len;
            var strp = 1;
            if (strip > 0)strp  = strip;
            for (int i=0; i<mats_len;i++)
            {
                if (i % strp == 0)
                {
                    var err = double.MaxValue;
                    var j_min = 0;
                    for (int j = 0; j < mats.Length; j++)
                    {
                        bool wall_i = false;
                        // if (i > 200)
                        wall_i = j > i;
                        // if (i < 200) wall_i = j<i;
                        if (Math.Abs(i - j) > wind && Math.Abs(i - j) < wind_max)
                        {
                            var cur_err = deviation_light(mats[i] - mats[j]);
                            //Console.WriteLine(cur_err);
                            if (cur_err < err)
                            {
                                err = cur_err;
                                j_min = j;
                            }
                        }
                    }

                    mats_diff[i] = mats[i] - mats[j_min];
                    /*CvInvoke.PutText(mats_diff[i], 
                        j_min.ToString(),
                        new Point(100, 100),
                        FontFace.HersheyScriptSimplex,
                        4, new MCvScalar(255));
                    CvInvoke.Imshow("diff", mats_diff[i]);
                    CvInvoke.WaitKey();*/
                    GC.Collect();
                    Console.WriteLine("comp_diff...      di: " + (j_min-i)+";   j_min: "+ j_min  + ";   " + i + "/" + mats.Length);
                }
            }

            return mats_diff;
        }

        static double deviation_light_old(Mat mat)
        {
            var im = mat.ToImage<Bgr, byte>();
            //dev= im.GetAverage().Red;
            int pres = 3;
            var r = //im.GetAverage().Red + 
                im.GetAverage().Green + im.GetAverage().Blue;
            return r;
        }
        static double deviation_light(Mat mat)
        {
            var im = mat.ToImage<Gray, byte>();
            var r = im.GetAverage();
            return r.Intensity;
        }


        static public double deviation_light_gauss(Mat mat,int ind = 0)//bin_ связь с config.thresh
        {

            var dev = 0d;
            var gauss = new Mat();
            var im = mat.ToImage<Gray, byte>();
            CvInvoke.GaussianBlur(im, gauss, new Size(17, 17), 8);
           // CvInvoke.GaussianBlur(gauss, gauss, new Size(13, 13), 7);
           // CvInvoke.Imshow("gauss", gauss);
            CvInvoke.Threshold(gauss, gauss, 10, 255, ThresholdType.Binary);

            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));
            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Ellipse , new Size(5, 5), new Point(1, 1));

            CvInvoke.MorphologyEx(gauss, gauss, MorphOp.Dilate, kernel5, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
            var im_g = gauss.ToImage<Gray, byte>();
            var k1 = im_g.CountNonzero()[0];
            //Console.WriteLine(k1);

            im -= im_g;
            dev = im.GetAverage().Intensity;
            //CvInvoke.AdaptiveThreshold(gauss, gauss, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 13, 20);
           /* CvInvoke.PutText(gauss,
                    ind+" "+dev.ToString(),
                    new Point(100, 100),
                    FontFace.HersheyScriptSimplex,
                    4, new MCvScalar(255));
            CvInvoke.Imshow("thresh", gauss);
            CvInvoke.WaitKey();*/
            return dev;
        }
    }
}
