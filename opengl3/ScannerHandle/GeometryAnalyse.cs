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
        static int count_1 = 0;
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

        public static System.Drawing.PointF[] compPointsInsideRectWarp(System.Drawing.PointF[] points2d, Size gridSize,Mat orig = null)
        {
            System.Drawing.PointF[] points3d = new System.Drawing.PointF[]
            {
                new System.Drawing.PointF(0,0),
                new System.Drawing.PointF(0,100),
                new System.Drawing.PointF(100,0),
                new System.Drawing.PointF(100,100)
            };
            var persp_Norm = CvInvoke.GetPerspectiveTransform(new VectorOfPointF(points3d), new VectorOfPointF(points2d));
            return CvInvoke.PerspectiveTransform(generatePsInsideRect(gridSize),persp_Norm);           
        }

        public static Mat findCirclesIter(Mat mat, ref System.Drawing.PointF[] corn, Size size_patt)
        {
            int offset_xy = 50;
            var points3d = new System.Drawing.PointF[]
            {
                    new System.Drawing.PointF(offset_xy,offset_xy),
                    new System.Drawing.PointF(offset_xy*size_patt.Width+offset_xy,offset_xy),
                    new System.Drawing.PointF(offset_xy,offset_xy*size_patt.Height+offset_xy),                    
                    new System.Drawing.PointF(offset_xy*size_patt.Width+offset_xy,offset_xy*size_patt.Height+offset_xy)       
            };

            var matDraw = FindCircles.findCircles(mat,ref corn, size_patt);
            if(matDraw == null)
                return null;
            //CvInvoke.Imshow("find1"+ count_1, matDraw);

            count_1++;
            var points2d = UtilOpenCV.takeGabObp(corn, size_patt);
            var orig1 = mat.Clone();
            UtilOpenCV.drawPointsF(orig1, points2d, 0, 255, 0,0,true);
            var persp_Norm = CvInvoke.GetPerspectiveTransform(new VectorOfPointF(points3d), new VectorOfPointF(points2d));
            var im_pers = mat.Clone();
            var persp_Norm_inv = new Mat();
            CvInvoke.Invert(persp_Norm, persp_Norm_inv, DecompMethod.LU);
            CvInvoke.WarpPerspective(mat, im_pers, persp_Norm_inv, 
                new Size(offset_xy * size_patt.Width + 2* offset_xy, offset_xy * size_patt.Height + 2*offset_xy),
                Inter.Linear,Warp.Default,BorderType.Replicate);           
            var matDraw_pers = FindCircles.findCircles(im_pers,ref corn, size_patt);

            
            //CvInvoke.Imshow("pers1" + count_1, matDraw_pers);
            count_1++;
            //CvInvoke.Imshow("pers1", matDraw_pers);

            corn = CvInvoke.PerspectiveTransform(corn, persp_Norm);
            UtilOpenCV.drawPointsF(mat, corn, 255, 0, 0, 2, true);
            return matDraw;
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
                    ps.Add(new System.Drawing.PointF(-(float)p[0, 0], (float)p[1, 0]));

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
