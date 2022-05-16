using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Descriptor
    {
        public float[] data;
        public int ind_desc;
        public int ind_im;
        public MKeyPoint keyPoint;
        public Descriptor( float[] _data, int _ind_desc, int _ind_im, MKeyPoint _keyPoint)
        {
            data = _data;
            ind_desc = _ind_desc;
            ind_im = _ind_im;
            keyPoint = _keyPoint;
        }
        static public Descriptor[] toDescriptors(VectorOfKeyPoint _keyPoints,Mat desk,int im)
        {

            var allData = (float[,])desk.GetData();
            if(allData==null)
            {
                return null;
            }
            var desks = new Descriptor[allData.GetLength(0)];
            for (int i=0; i<allData.GetLength(0);i++)
            {
                var _data = new float[allData.GetLength(1)];
                for (int j= 0; j < allData.GetLength(1); j++)
                {
                    _data[j] = allData[i, j];
                }
                desks[i] = new Descriptor(_data, i, im, _keyPoints[i]);
            }
            return desks;
        }
        static public double Diff(Descriptor desc1, Descriptor desc2)
        {
            float e = 0;
            for(int i=0; i<desc1.data.Length;i++)
            {
                var diff = desc1.data[i] - desc2.data[i];
                e += diff * diff;
            }
            return Math.Sqrt(e);
        }
    }
    public class Features
    {
        public VectorOfKeyPoint kps1;
        public VectorOfKeyPoint kps2;

        public VectorOfPointF ps1;
        public VectorOfPointF ps2;

        public VectorOfDMatch mchs;

        public System.Drawing.PointF[] mps1;
        public System.Drawing.PointF[] mps2;

        public Descriptor[] desks1;
        public Descriptor[] desks2;


        public Features()
        {

        }
        public VectorOfPointF keyToPoint(VectorOfKeyPoint kps)
        {
            var ps = new VectorOfPointF();
            for(int i=0; i<kps.Size;i++)
            {
                ps.Push(new System.Drawing.PointF[] { kps[i].Point });
            }
            return ps;
        }
        public MKeyPoint[] drawDescriptors(ref Mat mat)
        {
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector(50);
            var detector_SURF = new Emgu.CV.Features2D.FastFeatureDetector();
            var kp = detector_ORB.Detect(mat);



            // matcher.Match()
            var desc_brief = new Emgu.CV.XFeatures2D.BriefDescriptorExtractor();
            //new VectorOfKeyPoint();
            var descrs = new Mat();


            // desc_brief.DetectAndCompute(mat, null, kp, descrs, false);
            //var mat_desc = new Mat();
            for (int i = 0; i < kp.Length; i++)
            {
                CvInvoke.DrawMarker(
                    mat,
                    new Point((int)kp[i].Point.X, (int)kp[i].Point.Y),
                    new MCvScalar(0, 0, 255),
                    MarkerTypes.Cross,
                    4,
                    1);

            }

            return kp;
        }

        public Mat drawDescriptorsMatch(ref Mat mat1, ref Mat mat2)
        {
            var kps1 = new VectorOfKeyPoint();
            var desk1 = new Mat();
            var kps2 = new VectorOfKeyPoint();
            var desk2 = new Mat();
            //var detector = new Emgu.CV.Features2D.ORBDetector();
            var detector = new Emgu.CV.Features2D.SIFT();
            detector.DetectAndCompute(mat1, null, kps1, desk1, false);
            detector.DetectAndCompute(mat2, null, kps2, desk2, false);

           /* var matcherBF = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.L1, false);

            var ip = new Emgu.CV.Flann.KdTreeIndexParams(5);
            var sp = new Emgu.CV.Flann.SearchParams(50);
            var matcher = new Emgu.CV.Features2D.FlannBasedMatcher(ip, sp);*/

            //var matcherFlann = new Emgu.CV.Features2D.
           // var matches = new VectorOfDMatch();

            //matcherBF.Match(desk1, desk2, matches);
            //matches = matchBF(desk1, desk2);
            // matches = matchBF(Descriptor.toDescriptors(kps1,desk1,1), Descriptor.toDescriptors(kps2, desk2,2));

            this.desks1 = Descriptor.toDescriptors(kps1, desk1, 1);
            this.desks2 = Descriptor.toDescriptors(kps2, desk2, 2);

           
           var matches = matchEpiline(this.desks1, this.desks2);
            
            
               
            //prin.t("_______________");
            var mat3 = new Mat();
            try
            {
                if(matches==null)
                {
                    return null;
                }
                /*if (matches.Size > 10)
                {
                    matches = new VectorOfDMatch(new MDMatch[] { matches[5], matches[6], matches[7], matches[8], matches[9] });
                }*/
                this.kps1 = kps1;
                this.kps2 = kps2;

                this.ps1 = keyToPoint(kps1);
                this.ps2 = keyToPoint(kps2);

                this.mchs = matches;

                // matcher.KnnMatch(desk1, desk2, matches, 2);
                Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255), null,Emgu.CV.Features2D.Features2DToolbox.KeypointDrawType.NotDrawSinglePoints) ;
            }
            catch
            {
                Emgu.CV.Features2D.Features2DToolbox.DrawKeypoints(mat1, kps1, mat3, new Bgr(255, 0, 0));

            }
            return mat3;
        }

        static VectorOfDMatch matchEpiline(Descriptor[] desk1, Descriptor[] desk2)
        {
            var w = 400;
            var h = 400;
            var desk_line1 = new Descriptor[h][];
            var desk_line2 = new Descriptor[h][];
            if (desk1 == null || desk2 == null)
            {
                return null;               
            }
            else
            {
                if (desk1.Length == 0 || desk2.Length == 0)
                {
                    return null;
                }
            }
            VectorOfDMatch matches = new VectorOfDMatch();
            for (int i = 0; i < desk1.Length; i++)
            {
                var y = (int)desk1[i].keyPoint.Point.Y;
                var lr = desk_line1[y];
                if(lr == null)
                {
                    lr = new Descriptor[0];
                }
                var lr_l =  lr.ToList();
                lr_l.Add(desk1[i]);
                desk_line1[y] = lr_l.ToArray();
            }
            for (int i = 0; i < desk2.Length; i++)
            {
                var y = (int)desk2[i].keyPoint.Point.Y;
                var lr = desk_line2[y];
                if (lr == null)
                {
                    lr = new Descriptor[0];
                }
                var lr_l = lr.ToList();
                lr_l.Add(desk2[i]);
                desk_line2[y] = lr_l.ToArray();
            }

            for(int y=0; y<desk_line1.Length;y++)
            {
                if(desk_line1[y]!=null && desk_line2[y] != null)
                {
                    //prin.t(y + " " + desk_line1[y].Length + " " + desk_line2[y].Length);
                    if (desk_line1[y].Length >0 && desk_line2[y].Length > 0)
                    {
                        matches.Push(matchBF(desk_line1[y], desk_line2[y]));
                        
                    }
                }
            }

            return matches;
        }
        static VectorOfDMatch matchBF(Descriptor[] desk1, Descriptor[] desk2)
        {
            var mDMatchs = new List<MDMatch>();
            var es = new double[desk1.Length, desk2.Length];
            for (int i = 0; i < desk1.Length; i++)
            {
                var min_ind = 0;
                double min_e = double.MaxValue;
                for (int j = 0; j < desk2.Length; j++)
                {
                    es[i, j] = Descriptor.Diff(desk1[i], desk2[j]);
                    if (es[i, j] < min_e)
                    {
                        min_e = es[i, j];
                        min_ind = j;
                    }
                }
                var match = new MDMatch();
                match.ImgIdx = 0;
                match.TrainIdx = desk1[i].ind_desc;
                match.QueryIdx = desk2[min_ind].ind_desc;
                mDMatchs.Add(match);
            }
            return new VectorOfDMatch(mDMatchs.ToArray());
        }
        public float[] reconstuctScene(StereoCameraCV stereoCam ,Descriptor[] desk1, Descriptor[] desk2, VectorOfDMatch matches)
        {
            if(matches==null || desk1== null || desk2 == null)
            {
                return null;
            }
            var ps1 = new System.Drawing.PointF[matches.Size];
            var ps2 = new System.Drawing.PointF[matches.Size];
            for(int i=0; i<matches.Size;i++)
            {
                var m = matches[i];
                ps1[i] = desk1[m.TrainIdx].keyPoint.Point;                
                ps2[i] = desk2[m.QueryIdx].keyPoint.Point;
                //prin.t(ps1[i].Y - ps2[i].Y);
                //ps1[i].Y = 400 - ps1[i].Y;
               // ps2[i].Y = 400 - ps2[i].Y;
            }
            this.mps1 = ps1;
            this.mps2 = ps2;
            //prin.t("ps_________________");
            var p4s = new Mat();
            /*prin.t("prM1: ");
            prin.t(stereoCam.prM1);
            prin.t("p1: ");
            prin.t(stereoCam.p1);
            prin.t("stereoCam.cameraCVs[0].matrixCam: ");
            prin.t(stereoCam.cameraCVs[0].matrixCam);
            prin.t("stereoCam.cameraCVs[0].matrixScene: ");
            prin.t(stereoCam.cameraCVs[0].matrixScene);
            prin.t("stereoCam.cameraCVs[0].cameramatrix: ");
            prin.t(stereoCam.cameraCVs[0].cameramatrix);
            prin.t("______________________________________ ");*/
            //CvInvoke.TriangulatePoints(stereoCam.prM1, stereoCam.prM2, new VectorOfPointF(ps1), new VectorOfPointF(ps2),p4s);
            try
            {
                CvInvoke.TriangulatePoints(stereoCam.prM1, stereoCam.prM2, new VectorOfPointF(ps1), new VectorOfPointF(ps2), p4s);
            }
            catch
            {

            }
            if(p4s.Size.Width==0 && p4s.Size.Height == 0)
            {
                return null;
            }
            return reconstrToMesh(stereoCam,p4s);
        }

        static float[] reconstrToMesh(StereoCameraCV stereoCam,Mat p4s)
        {
            var mesh = new List<float>();
            var pdata = (float[,])p4s.GetData();
            if(pdata==null)
            {
                return null;
            }
            for(int i=0; i<pdata.GetLength(1); i++)
            {

                if (pdata[3,i]!=0)
                {
                    var x = pdata[0, i] / pdata[3, i];
                    var y = pdata[1, i] / pdata[3, i];
                    var z = pdata[2, i] / pdata[3, i];
                    mesh.Add(x);
                    mesh.Add(y);
                    mesh.Add(z);
                }
            }
            return mesh.ToArray();
        }

        static public float[] pointsForLines(System.Drawing.PointF[] ps, CameraCV cam, float z = 600)
        {
            var invmx = cam.matrixCS;
            if(ps==null)
            {
                return null;
            }
            var dataP = new float[ps.Length * 3];
            var j = 0;
            for(int i=0; i<ps.Length;i++)
            {
                var p = point3DfromCam(ps[i], cam, z);
               
                var v4 = new Matrix<double>(new double[] { p[0], p[1], p[2], 1 });
                var p_s =  invmx * v4;
               // prin.t(p_s);
                dataP[j] = (float)p_s[0,0]; j++;
                dataP[j] = (float)p_s[1, 0]; j++;
                dataP[j] = (float)p_s[2, 0]; j++;

            }
           // prin.t("______");
            return dataP;
        }

        static float[] point3DfromCam(System.Drawing.PointF _p, CameraCV cam,float z)
        {
            /*var A = cam.cameramatrix;
            var fx = A[0, 0];
            var fy = A[1, 1];
            var cx = A[0, 2];
            var cy = A[1, 2];
            var u = _p.X;
            var v = _p.Y;

            var x = (u - cx)/ fx;
            var y = (v - cy)/ fy;
             x *= z;
             y *= z;*/
            var p = new Matr3x3f(cam.cameramatrix_inv) * new Vert3f(_p.X, _p.Y, 1);
            return new float[] {  p.x * z, p.y * z, p.z * z };
        }

        static VectorOfDMatch matchBF(Mat desk1, Mat desk2)
        {
            var mDMatchs = new List<MDMatch>();
            var data1 = (float[,])desk1.GetData();
            var data2 = (float[,])desk2.GetData();
            var es = new double[data1.GetLength(0), data2.GetLength(0)];
            for (int i=0; i<data1.GetLength(0);i++)
            {
                var min_ind = 0;
                double min_e = 1000000;
                for (int j = 0; j < data2.GetLength(0); j++)
                {

                    float e = 0;
                    for (int k = 0; k < data2.GetLength(1); k++)
                    {
                        //Console.WriteLine(data1.GetLength(0) + " d10 " + data1.GetLength(1) + " d11 " + data2.GetLength(0) + " d20 " + data2.GetLength(1) + " d21 " + k + " k " + i + " i " + j + " j ");
                        var diff = data1[i, k] - data2[j, k];
                        e += diff * diff;
                    }
                    es[i,j] =  Math.Sqrt((double)e);
                    if(es[i, j]< min_e)
                    {
                        min_e = es[i, j];
                        min_ind = j;
                    }
                }

                var match = new MDMatch();
                match.ImgIdx = 0;
                match.TrainIdx = i;
                match.QueryIdx = min_ind;
               // prin.t(match.TrainIdx + " " + match.QueryIdx);
                mDMatchs.Add(match);
            }

            //prin.t(es);

            return new VectorOfDMatch(mDMatchs.ToArray());
        }

        static public Mat[] disparMap(Mat imL, Mat imR, int maxDisp, int blockSize)
        {
            var grayL = imL.ToImage<Gray, byte>();
            var grayR = imR.ToImage<Gray, byte>();
            //CvInvoke.Rotate(grayL, grayL, RotateFlags.Rotate90Clockwise);
            //CvInvoke.Rotate(grayR, grayR, RotateFlags.Rotate90Clockwise);
            //prin.t(grayL.Mat);
            var dimL = (byte[,])grayL.Mat.GetData();
            var dimR = (byte[,])grayR.Mat.GetData();
            var line1 = new int [dimL.GetLength(1)];
            var line2 = new int[dimL.GetLength(1)];
            var dispMap = new byte[dimL.GetLength(0), dimL.GetLength(1)];
            var diff2Map = new byte[dimL.GetLength(0), dimL.GetLength(1)];
            for (int i=0; i < dimL.GetLength(0); i++)
            {
                for (int j = 0; j < line1.Length; j++)
                {
                    line1[j] = dimL[i, j];
                    line2[j] = dimR[i, j];
                }
                var dispLine = bmatcherLine(line1, line2,maxDisp,blockSize);
                for (int j = 0; j < line1.Length; j++)
                {
                    if(dispLine[j]!=null)
                    {
                        var disp = dispLine[j][0];
                        diff2Map[i, j] = (byte)dispLine[j][1];
                        if (disp > 255)
                        {
                            dispMap[i, j] = 255;
                        }
                        else if (disp < 0)
                        {
                            dispMap[i, j] = 0;
                        }
                        else
                        {
                            dispMap[i, j] = (byte)disp;
                        }
                    }
                }
            }
            var matrD = new Matrix<byte>(dispMap);
            var matrD2 = new Matrix<byte>(diff2Map);
            return new Mat[] { matrD.Mat , matrD2.Mat };
        }
        static public Mat[] disparMap_3d(Mat imL, Mat imR, int maxDisp, int blockSize)
        {
            var grayL = imL.ToImage<Gray, byte>();
            var grayR = imR.ToImage<Gray, byte>();
            CvInvoke.Rotate(grayL, grayL, RotateFlags.Rotate90Clockwise);
            CvInvoke.Rotate(grayR, grayR, RotateFlags.Rotate90Clockwise);
            //prin.t(grayL.Mat);
            var dimL = (byte[,])grayL.Mat.GetData();
            var dimR = (byte[,])grayR.Mat.GetData();
            
            var dispMap = new byte[dimL.GetLength(0), dimL.GetLength(1)];
            var diff2Map = new byte[dimL.GetLength(0), dimL.GetLength(1)];
            var mat1 = new int[dimL.GetLength(0), dimL.GetLength(1)];
            var mat2 = new int[dimL.GetLength(0), dimL.GetLength(1)];
            for(int i=0; i< mat1.GetLength(0);i++)
            {
                for (int j = 0; j < mat1.GetLength(1); j++)
                {
                    mat1[i, j] = dimL[i, j];
                    mat2[i, j] = dimR[i, j];
                }
            }
            var data = bmatcherImage(mat1, mat2, maxDisp, blockSize);

            for (int i = 0; i < mat1.GetLength(0); i++)
            {
                for (int j = 0; j < mat1.GetLength(1); j++)
                {
                    if(data[i, j] != null)
                    {
                        dispMap[i, j] = (byte)data[i, j][0];
                        diff2Map[i, j] = (byte)data[i, j][1];
                    }
                    
                }
            }
 
            var matrD = new Matrix<byte>(dispMap);
            var matrD2 = new Matrix<byte>(diff2Map);
            return new Mat[] { matrD.Mat, matrD2.Mat };
        }
        #region 2d
        static int[][] bmatcherLine(int[] line1, int[] line2, int maxDisp, int blockSize)
        {
            var disp_line = new int[line1.Length][];
            var wind = maxDisp;
            if(blockSize>maxDisp)
            {
                wind = blockSize;
            }
            wind++;
            for (int i = wind; i<line1.Length-wind; i++)
            {
                var block = takeBlok(line1, i, blockSize);
                var batch = takeBlok(line2, i, maxDisp);
                disp_line[i] = disp(block, batch);
            }
            return disp_line;
        }
        static int[] takeBlok(int[] line, int indB, int blockSize)
        {
            var bl = new int[2*blockSize+1];
            
            for(int i = -blockSize; i < bl.Length - blockSize; i++)
            {
                //prin.t(indB + i);
                bl[i+ blockSize] = line[indB + i];
            }
            return bl;
        }

        static int compDiff(int[] block1, int[] block2)
        {
            
            int diff = 0;
            int min = int.MaxValue;
            for(int i =0; i<block1.Length;i++)
            {
                var d = block1[i] - block2[i];
                diff += d;
                if(d<min)
                {
                    min = d;
                }
            }
            if(diff<0)
            {
                return - diff + min* block1.Length;
            }
            return diff - min * block1.Length;
        }
        static int[] disp(int[] block, int[] batch)
        {
            var min = int.MaxValue;
            var disp = 0;
            var wind = (block.Length - 1) / 2;
            for (int i= wind; i< batch.Length-wind; i++)
            {
                var block2 = takeBlok(batch, i, wind);
                var en = compDiff(block, block2);
                if(en<min)
                {
                    min = en;
                    disp = i;
                }
            }

            return new int[] { disp, diff2(takeBlok(batch, disp, wind)) };
        }
        static int diff2(int[] block)
        {
            
            var diff = new int[block.Length-1];
            for(int i=0; i<diff.Length;i++)
            {
                diff[i] = block[i] - block[i+1];
            }

            int min = int.MaxValue;
            int diff_sum = 0;
            var d = 0;
            for (int i = 0; i < diff.Length - 1; i++)
            {
                d = diff[i] - diff[i + 1];
                diff_sum += d;
                if (d < min)
                {
                    min = d;
                }
            }
            if (diff_sum < 0)
            {
                return -diff_sum + min * (diff.Length - 1);
            }
            return diff_sum - min * (diff.Length - 1);
        }
        #endregion

        static int[,][] bmatcherImage(int[,] im1, int[,] im2, int maxDisp, int blockSize)
        {
            var disp_line = new int[im1.GetLength(0), im1.GetLength(1)][];
            var wind = maxDisp;
            if (blockSize > maxDisp)
            {
                wind = blockSize;
            }
            wind++;
            //wind++;
            for (int i = wind; i < im1.GetLength(0) - wind; i++)
            {
                for (int j = wind; j < im1.GetLength(1) - wind; j++)
                {
                    var block = takeBlok3d(im1, i,j, blockSize, blockSize);
                    var batch = takeBlok3d(im2, i,j, maxDisp, blockSize);
                    disp_line[i,j] = disp3d(block, batch);
                }
            }
            return disp_line;
        }
        static int[,] takeBlok3d(int[,] im, int x, int y, int xSize,int ySize)
        {
            var dimx = 2 * xSize + 1;
            var dimy = 2 * ySize + 1;
            var bl = new int[dimx, dimy];
           // Console.WriteLine("_________________"+x+" "+y);
            for (int i = -xSize; i < bl.GetLength(0) - xSize; i++)
            {
                for (int j = -ySize; j < bl.GetLength(1) - ySize; j++)
                {
                    
                    //Console.WriteLine("vlock size: " + bl.GetLength(0) + " " + bl.GetLength(1)+ "im size: " + im.GetLength(0) + " " + im.GetLength(1));
                    //Console.WriteLine("i:  " + (i + xSize) + " j:  " + (j + ySize) + " x : " + (x + i) + " y : " + (y + j));
                    bl[i + xSize, j + ySize] = im[x + i, y + j];
                }
            }
            return bl;
        }

        static int compDiff3d(int[,] block1, int[,] block2)
        {

            int diff = 0;
            int min = int.MaxValue;
            int w = block1.GetLength(0);
            int h = block1.GetLength(1);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var d = block1[i,j] - block2[i,j];
                    diff += d;
                    if (d < min)
                    {
                        min = d;
                    }
                }
            }
            if (diff < 0)
            {
                return -diff + min * (w*h);
            }
            return diff - min * (w * h);
        }
        static int[] disp3d(int[,] block, int[,] batch)
        {
            var min = int.MaxValue;
            var disp = 0;
            var xSize = (block.GetLength(0) - 1) / 2;
            var ySizeBatch = (batch.GetLength(1) - 1) / 2;
            var xdimBatch = batch.GetLength(0);
            for (int i = xSize; i < xdimBatch - xSize; i++)
            {

                var block2 = takeBlok3d(batch, i, ySizeBatch, xSize, xSize);
                var en = compDiff3d(block, block2);
                if (en < min)
                {
                    min = en;
                    disp = i;
                }
            }

            return new int[] { disp, diff2_3d(takeBlok3d(batch, disp, ySizeBatch, xSize, xSize)) };
        }
        static int diff2_3d(int[,] block)
        {
            int w = block.GetLength(0);
            int h = block.GetLength(1);
            var diff = new int[w - 1,h];
            for (int i = 0; i < w-1; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    diff[i,j] = block[i,j] - block[i + 1,j];
                }
            }

            int min = int.MaxValue;
            int diff_sum = 0;
            var d = 0;
            for (int i = 0; i < w - 2; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    d = diff[i,j] - diff[i + 1,j];
                    diff_sum += d;
                    if (d < min)
                    {
                        min = d;
                    }
                }
            }
            if (diff_sum < 0)
            {
                return -diff_sum + min * (w-2)*h;
            }
            return diff_sum - min * (w - 2) * h;
        }
    }

    

}
