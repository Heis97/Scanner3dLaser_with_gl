﻿using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct trsc
    {
        public Point3d_GL transl { get; set; }
        public Point3d_GL rotate { get; set; }
        public float scale { get; set; }
        public Matrix4x4f matr { get; set; }
        public trsc(Point3d_GL _transl, Point3d_GL _rotate, float _scale)
        {
            transl = _transl.Clone();
            rotate = _rotate.Clone();
            scale = _scale;
            matr = Matrix4x4f.Identity;
        }
        public trsc(double x, double y, double z, double rx, double ry, double rz, float _scale)
        {
            transl = new Point3d_GL(x, y, z);
            rotate = new Point3d_GL(rx, ry, rz);
            scale = _scale;
            matr = Matrix4x4f.Identity;
        }
        public trsc(Matrix4x4f _matr)
        {
            transl = new Point3d_GL(0, 0, 0);//notExist

            rotate = new Point3d_GL(0, 0, 0);
            scale = 1;
            matr = _matr;
        }

        public Matrix4x4f getModelMatrix()
        {
            if (!transl.exist)
            {
                return matr;
            }
            return Matrix4x4f.Translated((float)transl.x, (float)transl.y, (float)transl.z) *
                Matrix4x4f.RotatedX((float)rotate.x) *
                Matrix4x4f.RotatedY((float)rotate.y) *
                Matrix4x4f.RotatedZ((float)rotate.z) *
                Matrix4x4f.Scaled(scale, scale, scale);
        }


        public static Matrix4x4f toGLmatrix(Matrix<double> matrixCV)
        {
            var matrixCV_T = matrixCV.Transpose();
            var matrixGL = Matrix4x4f.Identity;
            for (int i = 0; i < matrixCV_T.Width; i++)
            {
                for (int j = 0; j < matrixCV_T.Height; j++)
                {
                    matrixGL[(uint)i, (uint)j] = (float)matrixCV_T[i, j];
                }
            }
            return matrixGL;
        }
    }

    public class TransRotZoom
    {
        public enum TRZtype { Master, Slave, Const }
        public double zoom;
        public double xRot;
        public double yRot;
        public double zRot;
        public double off_x;
        public double off_y;
        public double off_z;
        public double fovx = 52;
        public double fovy = 53;
        public TRZtype type;
        public viewType viewType_;
        public int id;
        public int id_m;
        public bool visible;
        public bool robot_camera;
        public Rectangle rect;
        public DateTime dateTime;
        public TransRotZoom consttransf;
        public CameraCV cameraCV;
        public string view_3d = "-1";
        public Matrix4x4f const_trz;
        public Matrix4x4f robot_matr = new Matrix4x4f();
        public TransRotZoom(Rectangle _rect, int _id)
        {
            zoom = 30;
            xRot = 0;
            yRot = 0;
            zRot = 0;
            off_x = 0;
            off_y = 0;
            off_z = -2;
            rect = _rect;
            id = _id;
            type = TRZtype.Master;
            viewType_ = viewType.Ortho;
            visible = false;
            cameraCV =  new CameraCV(UtilOpenCV.matrixForCamera(new Size(rect.Width, rect.Height), fovx), new Matrix<double>(5, 1), new Size(rect.Width, rect.Height));
            robot_camera = false;
            robot_matr = new Matrix4x4f();
        }

        public TransRotZoom(Rectangle _rect, int _id, Vertex3d rotVer, Vertex3d transVer, int _idMast)
        {
            zoom = 1;
            xRot = 0;
            yRot = 0;
            zRot = 0;
            off_x = 0;
            off_y = 0;
            off_z = 400;
            rect = _rect;
            id = _id;
            id_m = _idMast;
            type = TRZtype.Slave;
            consttransf = new TransRotZoom(rotVer, transVer);
            viewType_ = viewType.Ortho;
            visible = false;
            const_trz = new trsc(transVer.x, transVer.y, transVer.z, rotVer.x, rotVer.y, rotVer.z, 1).getModelMatrix();
            cameraCV = new CameraCV(UtilOpenCV.matrixForCamera(new Size(rect.Width, rect.Height), fovx), new Matrix<double>(5, 1), new Size(rect.Width, rect.Height));
            robot_camera = false;
        }
        public TransRotZoom(Vertex3d rotVer, Vertex3d transVer)
        {
            xRot = rotVer.x;
            yRot = rotVer.y;
            zRot = rotVer.z;
            off_x = transVer.x;
            off_y = transVer.y;
            off_z = transVer.z;
            type = TRZtype.Const;
            viewType_ = viewType.Perspective;
            visible = false;
            robot_camera = false;
        }

        public TransRotZoom getInfo(TransRotZoom[] transRotZooms)
        {
            switch (type)
            {
                case TRZtype.Master:
                    return this;

                case TRZtype.Slave:
                    var trz_m = transRotZooms[id_m];
                    var trz_info = new TransRotZoom();
                    trz_info.zoom = trz_m.zoom;
                    trz_info.off_x = trz_m.off_x;// + consttransf.off_x;
                    trz_info.off_y = trz_m.off_y;// + consttransf.off_y;
                    trz_info.off_z = trz_m.off_z;// + consttransf.off_z;
                    trz_info.xRot = trz_m.xRot;// + consttransf.xRot;
                    trz_info.yRot = trz_m.yRot;// + consttransf.yRot;
                    trz_info.zRot = trz_m.zRot;// + consttransf.zRot;
                    trz_info.viewType_ = trz_m.viewType_;
                    trz_info.rect = rect;
                    trz_info.visible = trz_m.visible;
                    trz_info.view_3d = view_3d;
                    trz_info.id = id;
                    trz_info.const_trz = const_trz;
                    trz_info.type = type;
                    trz_info.robot_camera = robot_camera;
                    return trz_info;
                default:
                    return null;
            }
        }
        public TransRotZoom(TransRotZoom _trz)
        {
            zoom = _trz.zoom;
            xRot = _trz.xRot;
            yRot = _trz.yRot;
            zRot = _trz.zRot;
            off_x = _trz.off_x;
            off_y = _trz.off_y;
            off_z = _trz.off_z;
            rect = _trz.rect;
            id = _trz.id;
            type = _trz.type;
        }
        public TransRotZoom()
        {

        }

        public TransRotZoom(string data)
        {
            var dt = data.Split();
            if (dt.Length < 7)
            {

            }
            xRot = Convert.ToDouble(dt[0]);
            yRot = Convert.ToDouble(dt[1]);
            zRot = Convert.ToDouble(dt[2]);
            off_x = Convert.ToDouble(dt[3]);
            off_y = Convert.ToDouble(dt[4]);
            off_z = Convert.ToDouble(dt[5]);
            zoom = Convert.ToDouble(dt[6]);
        }
        public TransRotZoom(double _xRot, double _yRot, double _zRot,
            double _off_x, double _off_y, double _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            off_x = _off_x;
            off_y = _off_y;
            off_z = _off_z;
            zoom = _zoom;
        }
        public void setTrz(double _xRot, double _yRot, double _zRot,
            double _off_x, double _off_y, double _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            off_x = _off_x;
            off_y = _off_y;
            off_z = _off_z;
            zoom = _zoom;
        }
        public TransRotZoom minusDelta(TransRotZoom trz)
        {
            var _xRot = xRot - trz.xRot;
            var _yRot = yRot - trz.yRot;
            var _zRot = zRot - trz.zRot;
            var _off_x = off_x - trz.off_x;
            var _off_y = off_y - trz.off_y;
            var _off_z = off_z - trz.off_z;
            var _zoom = zoom - trz.zoom;
            return new TransRotZoom(_xRot, _yRot, _zRot, _off_x, _off_y, _off_z, _zoom);
        }
        public void setTrz(TransRotZoom trz)
        {
            xRot = trz.xRot;
            yRot = trz.yRot;
            zRot = trz.zRot;
            off_x = trz.off_x;
            off_y = trz.off_y;
            off_z = trz.off_z;
            zoom = trz.zoom;
        }
        public override string ToString()
        {
            var pres = 4;
            return Math.Round( xRot,pres) + " " + Math.Round(yRot, pres) + " " + Math.Round(zRot, pres) + " "
                + Math.Round(off_x, pres) + " " + Math.Round(off_y, pres) + " " + Math.Round(off_z, pres) + " "
                + zoom + " " + viewType_ + " "+view_3d+" "+visible;
        }

        public static TransRotZoom operator -(TransRotZoom trz1, TransRotZoom trz2)
        {
            return new TransRotZoom(
                trz1.xRot - trz2.xRot,
                 trz1.yRot - trz2.yRot,
                  trz1.zRot - trz2.zRot,
                   trz1.off_x - trz2.off_x,
                   trz1.off_y - trz2.off_y,
                   trz1.off_z - trz2.off_z,
                   trz1.zoom - trz2.zoom
                  );
        }
        public void setxRot(double value)
        {
            xRot = value;
        }
        public void setyRot(double value)
        {
            yRot = value;
        }
        public void setzRot(double value)
        {
            zRot = value;
        }
        public void setRot(double valuex, double valuey, double valuez)
        {
            xRot = valuex;
            yRot = valuey;
            zRot = valuez;
        }
        static string get_matrix_parametrs(float fovy, float aspect,float near, float far)
        {
            float num = near * (float)Math.Tan(Angle.ToRadians(fovy / 2f));
            float num2 = num * aspect;

            float left = 0f - num2;
            float right = num2;
            float bottom = 0f - num;
            float top =  num;
            return "left: " + left + "; right: " + right + "; bottom: " + bottom + "; top: " + top + "; near: " + near + "; far: " + far;
        }
        static public Matrix4x4f[] getVPmatrix(TransRotZoom trz)
        {
            var _Pm = Matrix4x4f.Identity;
            var _Vm = Matrix4x4f.Identity;
            //Matrix4x4f.Perspective()
            if (trz.viewType_ == viewType.Perspective)
            {
                _Pm = Matrix4x4f.Perspective((float)trz.fovx, (float)trz.rect.Width / trz.rect.Height, 1f, 10000f);
                _Vm = Matrix4x4f.Translated((float)trz.off_x, -(float)trz.off_y, (float)trz.zoom * (float)trz.off_z) *
                    Matrix4x4f.RotatedX((float)trz.xRot) *
                    Matrix4x4f.RotatedY((float)trz.yRot) *
                    Matrix4x4f.RotatedZ((float)trz.zRot);
                //Console.WriteLine(_Pm.ToString());
                //Console.WriteLine(get_matrix_parametrs((float)trz.fovx, (float)trz.rect.Width / trz.rect.Height, 1f, 10000f));

            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                float window = (float)trz.zoom;
                float aspec = (float)trz.rect.Width / trz.rect.Height;
                _Pm = Matrix4x4f.Ortho(-window * aspec, window * aspec, -window, window, -10000f, 10000f);
                /* _Vm = Matrix4x4f.Translated((float)trz.off_x, -(float)trz.off_y, (float)trz.off_z) *
                     Matrix4x4f.RotatedX((float)trz.xRot) *
                     Matrix4x4f.RotatedY((float)trz.yRot) *
                     Matrix4x4f.RotatedZ((float)trz.zRot);*/

                _Vm =
                    Matrix4x4f.RotatedX((float)trz.xRot) *
                    Matrix4x4f.RotatedY((float)trz.yRot) *
                    Matrix4x4f.RotatedZ((float)trz.zRot) * Matrix4x4f.Translated((float)trz.off_x, -(float)trz.off_y, (float)trz.off_z);
            }
            if(trz.robot_camera)
            {
                _Pm = Matrix4x4f.Perspective((float)trz.fovx, (float)trz.rect.Width / trz.rect.Height, 1f, 10000f);
               /* _Vm = 
                    Matrix4x4f.RotatedZ((float)trz.xRot) *
                    Matrix4x4f.RotatedY((float)trz.yRot) *
                    Matrix4x4f.RotatedX((float)trz.zRot)*Matrix4x4f.Translated((float)trz.off_x, (float)trz.off_y, (float)trz.off_z) ;*/

                _Vm = trz.robot_matr;
            }

            if (trz.type == TRZtype.Slave) _Vm = trz.const_trz * _Vm;
            return new Matrix4x4f[] { _Pm, _Vm, _Pm * _Vm };
        }
    }
}
