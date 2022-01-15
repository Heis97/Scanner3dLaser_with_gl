using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class Features
    {
        public VectorOfKeyPoint kps1;
        public VectorOfKeyPoint kps2;

        public VectorOfPointF ps1;
        public VectorOfPointF ps2;
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
           
            /* prin.t(desk1);
             prin.t("________");
             prin.t(desk2);
             prin.t("________");
             prin.t(kps1);
             prin.t("________");
             prin.t(kps2);
             prin.t("________");

             prin.t("________");*/
            matches = deskDiff(desk1, desk2);
            //prin.t("_______________");
            var mat3 = new Mat();
            try
            {
                this.kps1 = kps1;
                this.kps2 = kps2;

                this.ps1 = keyToPoint(kps1);
                this.ps2 = keyToPoint(kps2);
               // matcher.KnnMatch(desk1, desk2, matches, 2);
                Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255));
            }
            catch
            {
                Emgu.CV.Features2D.Features2DToolbox.DrawKeypoints(mat1, kps1, mat3, new Bgr(255, 0, 0));

            }
            return mat3;
        }
        VectorOfDMatch deskDiff(Mat desk1, Mat desk2)
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
        
        public VectorOfDMatch epilineMatch(Mat mat1, Mat mat2, StereoCameraCV stereoCameraCV)
        {
            return null;
        }
    }

    

}
