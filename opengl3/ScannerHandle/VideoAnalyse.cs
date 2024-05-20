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



            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
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

                        CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);
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
            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }
        static public Scanner loadVideo_stereo(string filepath, Scanner scanner, ScannerConfig config, MainScanningForm form)
        {

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
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
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
                CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
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
                        //Console.WriteLine(f1 + " " + f2);


                        //CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);


                        //if(videoframe_count!= 100 && videoframe_count != 103 && videoframe_count != 104 && videoframe_count != 145 && videoframe_count != 146 && videoframe_count <149 && videoframe_count > 100)
                        {
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
            form.get_combo_im().Items.AddRange(frames_show.ToArray());
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }
        static public Scanner loadVideo_sing_cam(string filepath,MainScanningForm form, Scanner scanner = null, int strip = 1, bool calib = false)
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

            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
           // orig1 = scanner.cameraCV.undist(orig1);
            var fr_st_vid = new Frame(orig1, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            var pos_inc_cal = new List<double>();
            //comboImages.Items.Add(fr_st_vid);
            int buff_diff = 10;
            int buff_len = buff_diff + 1;
            var all_frames = all_frames1;
            if (scanner != null)
            {
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>() };
                scanner.pointCloud.graphicGL = form.GL1;
            }
            var enc_file = "";
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc_file = sr.ReadToEnd();
            }
            var inc_pos = scanner.enc_pos(enc_file, (int)all_frames);

            analys_sync(enc_path);
            var buffer_mat = new Mat();
            var im_orig = orig1.ToImage<Bgr, byte>();
            
            var im1_buff = new Mat();


            var im1_buff_list = new List<Mat>();
            for(int i=0; i< inc_pos.Length; i++)
            {
                Console.WriteLine(i+" "+inc_pos[i]);
            }
            //Console.WriteLine("start video_________");
            while (videoframe_count < all_frames/2+1)
            {
                Mat im1 = new Mat();
                while (!capture1.Read(im1)) { }
                if (scanner != null)
                {
                    var buffer_mat1 = im1.Clone();
                    //if (videoframe_count % strip == 0)
                    if (videoframe_count % strip == 0 && videoframe_count> buff_len)//&& videoframe_count > 37 && videoframe_count <173)
                    {
                        //var im1_or = im1.Clone();
                        im1 -= im1_buff_list[buff_len - buff_diff];
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
                        var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                        frames_show.Add(frame_d);
                        //Console.WriteLine(videoframe_count.ToString() + " " + inc_pos[videoframe_count].ToString());
                        if (calib)
                        {
                            //var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                            // frames_show.Add(frame_d);
                            pos_inc_cal.Add(inc_pos[videoframe_count]);
                            
                            scanner.addPointsSingLas_2d(im1, false, calib);
                        }
                        else scanner.addPointsLinLas_step(im1, im_orig, inc_pos[videoframe_count], PatternType.Mesh);

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
            //comboImages.Items.AddRange(frames_show.ToArray());
            Console.WriteLine("stop video_________");

            if (calib) scanner.calibrateLinearStep(Frame.getMats(frames_show.ToArray()), orig1, pos_inc_cal.ToArray(), PatternType.Mesh, form.GL1);

            //var mats = Frame.getMats(frames_show.ToArray());
            //var corn = Detection.detectLineDiff_corn_calibr(mats);

            //UtilOpenCV.drawPointsF(orig1, corn, 255, 0, 0, 2, true);
            //CvInvoke.Imshow("corn", orig1);
            return scanner;
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
            var all_frames = capture.GetCaptureProperty(CapProp.FrameCount);
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

        static int[] frames_max(int[,] data)
        {
            int analyse_len = 40;
            var end_data = new List<int[]>();
            for (int i = data.GetLength(0) - analyse_len; i < data.GetLength(0) - 1; i++)
            {
                end_data.Add(new int[] { data[i, 1], data[i, 2] });
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
                var fr_n = data[i, 1];
                var cam_n = data[i, 2];
                var time = data[i, 3];
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
        static int[,] analys_sync(string enc_path)
        {
            string enc;
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc = sr.ReadToEnd();
            }

            enc = enc.Replace("\r", "");
            var lines = enc.Split('\n');
            var enc_pos = new int[lines.Length, 8];
            int ind = 0;
            int st_time = 0;
            foreach (var line in lines)
            //for (int l = lines.Length-1; l>=0; l--)
            {
                //var line = lines[l];
                if (line.Length > 0)
                {
                    var vals = line.Trim().Split(' ');

                    if (vals.Length == 6)
                    {
                        for (int i = 0; i < vals.Length; i++)
                        //for (int i = vals.Length - 1; i >= 0; i--)
                        {
                            if (i == 3)
                            {
                                string time = "";
                                for (int j = vals[i].Length / 2; j < vals[i].Length; j++)
                                {
                                    time += vals[i][j];
                                }
                                vals[i] = time;
                            }
                            enc_pos[ind, i] = Convert.ToInt32(vals[i]);
                            if (i == 3)
                            {
                                enc_pos[ind, i] -= st_time;
                                if (ind == 0)
                                {
                                    st_time = enc_pos[ind, i];
                                    enc_pos[ind, i] = 0;
                                }

                            }
                        }
                        //Console.Write(vals[i] + ";");
                        /*Console.Write(enc_pos[ind, 3] );
                         if (enc_pos[ind, 5] == 1) Console.Write(enc_pos[ind, 4] + ";" + ";");
                         if (enc_pos[ind, 5] == 2) Console.Write(";" + enc_pos[ind, 4] + ";");


                         Console.WriteLine(' ');*/
                    }
                }
                ind++;
            }
            return enc_pos;
        }


        static Mat get_frame_video(string video_path, int frame)
        {
            var capture = new VideoCapture(video_path);
            var mat = new Mat();
            capture.SetCaptureProperty(CapProp.PosFrames, frame);
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
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
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
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
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

        static Mat[] diff_mats_bf(Mat[] mats,int wind)
        {
            var mats_diff = new Mat[mats.Length];
            for(int i=0; i<mats.Length;i++)
            {
                var err = double.MaxValue;
                var j_min = 0;
                for(int j=0; j<mats.Length;j++)
                {
                    if (Math.Abs(i - j) > wind)
                    {
                        var cur_err = deviation_light_gauss(mats[i] - mats[j]);
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
                    4, new MCvScalar(255));*/
                GC.Collect();
                Console.WriteLine("comp_diff...      " + i + "/" + mats.Length);
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


        static public double deviation_light_gauss(Mat mat)//bin_ связь с config.thresh
        {

            var dev = 0d;
            var gauss = new Mat();
            var im = mat.ToImage<Gray, byte>();
            CvInvoke.GaussianBlur(im, gauss, new Size(13, 13), 7);
            CvInvoke.GaussianBlur(gauss, gauss, new Size(13, 13), 7);
            CvInvoke.Imshow("gauss", gauss);
            CvInvoke.Threshold(gauss, gauss, 15, 255, ThresholdType.Binary);

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
            CvInvoke.Imshow("thresh", gauss);

            return dev;
        }
    }
}
