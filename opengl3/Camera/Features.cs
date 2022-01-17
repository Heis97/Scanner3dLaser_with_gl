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
            var matcherBF = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.L1, false);

            var ip = new Emgu.CV.Flann.KdTreeIndexParams(5);
            var sp = new Emgu.CV.Flann.SearchParams(50);
            var matcher = new Emgu.CV.Features2D.FlannBasedMatcher(ip, sp);

            //var matcherFlann = new Emgu.CV.Features2D.
            var matches = new VectorOfDMatch();

            //matcherBF.Match(desk1, desk2, matches);
            //matches = matchBF(desk1, desk2);
            // matches = matchBF(Descriptor.toDescriptors(kps1,desk1,1), Descriptor.toDescriptors(kps2, desk2,2));

            this.desks1 = Descriptor.toDescriptors(kps1, desk1, 1);
            this.desks2 = Descriptor.toDescriptors(kps2, desk2, 2);

            matches = matchEpiline(this.desks1, this.desks2);

            //prin.t("_______________");
            var mat3 = new Mat();
            try
            {
                this.kps1 = kps1;
                this.kps2 = kps2;

                this.ps1 = keyToPoint(kps1);
                this.ps2 = keyToPoint(kps2);

                this.mchs = matches;
               // matcher.KnnMatch(desk1, desk2, matches, 2);
                Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255));
            }
            catch
            {
                Emgu.CV.Features2D.Features2DToolbox.DrawKeypoints(mat1, kps1, mat3, new Bgr(255, 0, 0));

            }
            return mat3;
        }
        
        VectorOfDMatch matchEpiline(Descriptor[] desk1, Descriptor[] desk2)
        {
            var w = 400;
            var h = 400;
            var desk_line1 = new Descriptor[h][];
            var desk_line2 = new Descriptor[h][];

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
        VectorOfDMatch matchBF(Descriptor[] desk1, Descriptor[] desk2)
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
            var ps1 = new System.Drawing.PointF[matches.Size];
            var ps2 = new System.Drawing.PointF[matches.Size];
            for(int i=0; i<matches.Size;i++)
            {
                var m = matches[i];
                ps1[i] = desk1[m.TrainIdx].keyPoint.Point;
                ps2[i] = desk2[m.QueryIdx].keyPoint.Point;
                //prin.t(ps1[i].Y - ps2[i].Y);
            }
            this.mps1 = ps1;
            this.mps2 = ps2;
            //prin.t("ps_________________");
            var p4s = new Mat();
            CvInvoke.TriangulatePoints(stereoCam.prM1, stereoCam.prM2, new VectorOfPointF(ps1), new VectorOfPointF(ps2),p4s);
            
            return reconstrToMesh(p4s);
        }

        float[] reconstrToMesh(Mat p4s)
        {
            var mesh = new List<float>();
            var pdata = (float[,])p4s.GetData();
            for(int i=0; i<pdata.GetLength(1); i++)
            {
                if(pdata[3,i]!=0)
                {
                    mesh.Add(pdata[0, i] / pdata[3, i]);
                    mesh.Add(pdata[1, i] / pdata[3, i]);
                    mesh.Add(pdata[2, i] / pdata[3, i]);
                }
            }
            return mesh.ToArray();
        }

        public float[] pointsForLines(System.Drawing.PointF[] ps, CameraCV cam, float z = 600)
        {
            var invmx = cam.matrixScene;
            var dataP = new float[ps.Length * 3];
            var j = 0;
            for(int i=0; i<ps.Length;i++)
            {

                var p = point3DfromCam(ps[i], cam, z);
                var v4 = new Matrix<double>(new double[] { p[0], p[1], p[2], 1 });
                var p_s = invmx * v4;
                dataP[j] = (float)p_s[0,0]; j++;
                dataP[j] = (float)p_s[1, 0]; j++;
                dataP[j] = (float)p_s[2, 0]; j++;
            }
            return dataP;
        }

        float[] point3DfromCam(System.Drawing.PointF p, CameraCV cam,float z)
        {
            var A = cam.cameramatrix;
            var fx = A[0, 0];
            var fy = A[1, 1];
            var cx = A[0, 2];
            var cy = A[1, 2];
            var u = p.X;
            var v = p.Y;

            var x = (u - cx) / fx;
            var y = (v - cy) / fy;
            // var x = u  / fx;
            //   var y = v / fy;
             x *= z;
              y *= z;
            var k = 1f;
            return new float[] {  k*(float)x, k * (float)y, k * (float)z };
        }

        VectorOfDMatch matchBF(Mat desk1, Mat desk2)
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
        
        
    }

    

}
