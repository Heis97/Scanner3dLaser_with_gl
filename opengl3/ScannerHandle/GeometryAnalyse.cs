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
    public static class GeometryAnalyse
    {
        public static System.Drawing.PointF[] compPointsInsideRect(System.Drawing.PointF[] points2d, Size gridSize)
        {
            MCvPoint3D32f[] points3d = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(0,0,1),
                new MCvPoint3D32f(0,1,1),
                new MCvPoint3D32f(1,0,1),
                new MCvPoint3D32f(1,1,1)
            };
            var matr = homographyMatr(points3d, points2d);
            return generatePsInsideRect(gridSize, matr);
        }

        public static System.Drawing.PointF[] compPointsInsideRectWarp(System.Drawing.PointF[] points2d, Size gridSize)
        {
            System.Drawing.PointF[] points3d = new System.Drawing.PointF[]
            {
                new System.Drawing.PointF(0,0),
                new System.Drawing.PointF(0,1),
                new System.Drawing.PointF(1,0),
                new System.Drawing.PointF(1,1)
            };
            var persp_Norm = CvInvoke.GetPerspectiveTransform(new VectorOfPointF(points3d), new VectorOfPointF(points2d));

            return CvInvoke.PerspectiveTransform(generatePsInsideRect(gridSize), persp_Norm);           
        }

        public static System.Drawing.PointF[] findCirclesIter(Mat mat, Size size_patt, float markSize)
        {

            var points3d = new System.Drawing.PointF[]
            {
                    new System.Drawing.PointF(0,0),
                    new System.Drawing.PointF(100*size_patt.Width,0),
                    new System.Drawing.PointF(0,100*size_patt.Height),
                    new System.Drawing.PointF(100*size_patt.Width,100*size_patt.Height)
            };

            var len = size_patt.Width * size_patt.Height;
            var cornF = new System.Drawing.PointF[len];
            var matDraw = FindCircles.findCircles(mat, cornF, size_patt);
            var points2d = UtilOpenCV.takeGabObp(cornF, size_patt);


            var persp_Norm = CvInvoke.GetPerspectiveTransform(new VectorOfPointF(points3d), new VectorOfPointF(points2d));
            var im_pers = mat.Clone();
            // CvInvoke.PerspectiveTransform(mat, im_pers, persp_Norm);
            //prin.t(persp_Norm);
            CvInvoke.WarpPerspective(mat, im_pers, persp_Norm,new Size(mat.Size.Width*2, mat.Size.Height * 2), Inter.Linear,Warp.Default,BorderType.Replicate);//,new MCvScalar(127,255,255)
            //CvInvoke.Imshow("pers1", matDraw);
            var matDraw_pers = FindCircles.findCircles(im_pers, cornF, size_patt);
            //CvInvoke.Imshow("pers2", matDraw_pers);
            var persp_Norm_inv = new Mat();
            CvInvoke.Invert(persp_Norm, persp_Norm_inv, DecompMethod.LU);

            cornF = CvInvoke.PerspectiveTransform(cornF, persp_Norm_inv);

            return cornF;
        }

        static System.Drawing.PointF[] generatePsInsideRect(Size _gridSize, Matrix<double> matr=null)
        {
            List<System.Drawing.PointF> ps = new List<System.Drawing.PointF>();
            var gridSize = new Size(_gridSize.Height-1,_gridSize.Width-1);
            for (int i=0; i<=gridSize.Width;i++)
            {
                for (int j = 0; j <= gridSize.Height; j++)
                {
                    var p =  new Matrix<double>(new double[,] { { (double)i / (double)gridSize.Width }, { (double)j / (double)gridSize.Height }, { 1 } });
                    if (matr!=null)
                    {
                        p = matr * p;
                        
                    }
                    ps.Add(new System.Drawing.PointF((float)p[0, 0], (float)p[1, 0]));

                }
            }
            return ps.ToArray();
        }

        static Matrix<double> homographyMatr(MCvPoint3D32f[] points3d, System.Drawing.PointF[] points2d)
        {
            Matrix<double> matrix3d = new Matrix<double>(3, 3);
            Matrix<double> matrix2d = new Matrix<double>(3, 3);
            Matrix<double> matrix = new Matrix<double>(3, 3);
            if (points3d.Length > 3 && points2d.Length > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i].X;
                    matrix3d[i, 1] = points3d[i].Y;
                    matrix3d[i, 2] = 1;

                    matrix2d[i, 0] = points2d[i].X;
                    matrix2d[i, 1] = points2d[i].Y;
                    matrix2d[i, 2] = 1;
                }
                var K = new Matrix<double>(3, 3);
                
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix1 = K * matrix2d;
                //prin.t(matrix);
                matrix = matrix1;

               /* for (int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i + 1].X;
                    matrix3d[i, 1] = points3d[i + 1].Y;
                    matrix3d[i, 2] = 1;

                    matrix2d[i, 0] = points2d[i + 1].X;
                    matrix2d[i, 1] = points2d[i + 1].Y;
                    matrix2d[i, 2] = 1;
                }
                K = new Matrix<double>(3, 3);
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix2 = K * matrix2d;

                matrix = (matrix1 + matrix2) / 2;*/
            }
            return matrix.Transpose();
        }


    }
}
