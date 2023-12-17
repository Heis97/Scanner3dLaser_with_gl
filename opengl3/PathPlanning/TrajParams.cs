using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public class TrajParams
    {

        public double[] z;
        public double dz;
        [Description("Высота слоя")]
        [Category("Параметры траектории")]
        [DisplayName("dz")]
        public double dZ
        {
            get { return dz; }
            set { dz = value; comp_z(); }
        }


        public void comp_z()
        {
            if (layers <= 0)
            {
                return;
            }
            z = new double[layers];
            for (int i = 0; i < z.Length; i++)
            {
                z[i] = dz * (i + 1)+off_z;
            }
        }

        public int layers;
        [Description("Количество слоёв")]
        [Category("Параметры траектории")]
        [DisplayName("nz")]
        public int Layers
        {
            get { return layers; }
            set { layers = value; comp_z(); }
        }
        
        public double step;
        [Description("Шаг решётки")]
        [Category("Параметры траектории")]
        [DisplayName("step")]
        public double Step
        {
            get { return step; }
            set { step = value; }
        }
        public double vel;
        [Description("Скорость заполнения")]
        [Category("Параметры траектории")]
        [DisplayName("vel")]
        public double Vel
        {
            get { return vel; }
            set { vel = value; }
        }
        public double line_width;
        [Description("Толщина линий")]
        [Category("Параметры траектории")]
        [DisplayName("width")]
        public double Line_width
        {
            get { return line_width; }
            set { line_width = value; }
        }
        public double off_z;
        [Description("Смещение по z")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("off_z")]
        public double Off_z
        {
            get { return off_z; }
            set {off_z = value; }
        }
        public double off_x;
        public double Off_x
        {
            get { return off_x; }
            set { off_x = value; }
        }
        public double off_y;
        public double Off_y
        {
            get { return off_y; }
            set { off_y = value; }
        }
        //------------------------
        public double div_step;
        [Description("Разбиение траектории")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("div_step")]
        public double Div_step
        {
            get { return div_step; }
            set { div_step = value; }
        }

        public double ang_x;
        [Description("Поворот Дозатора вокруг z")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("ang_x")]
        public double Ang_x
        {
            get { return ang_x; }
            set { ang_x  = value; }
        }
        public double layers_angle;

        [Description("Стартовый угол слоёв")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("layers_start_ang")]
        
        public double Layers_angle
        {
            get { return layers_angle; }
            set { layers_angle = value; }
        }
        public double h_transf;
        [Description("Высота перехода внутри")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("h_transf")]
        public double H_transf
        {
            get { return h_transf; }
            set { h_transf = value; }
        }

        public double h_transf_out;
        [Description("Высота перехода снаружи")]
        [Category("Дополнительные параметры траектории")]
        [DisplayName("h_transf_out")]
        public double H_transf_out
        {
            get { return h_transf_out; }
            set { h_transf_out = value; }
        }

        public int w_smooth_ang;
        [Description("Окно сглаживания угла")]
        [Category("Параметры угла")]
        [DisplayName("w_smooth_ang")]
        public int W_smooth_ang
        {
            get { return w_smooth_ang; }
            set { w_smooth_ang = value; }
        }

        public double k_decr_ang;
        [Description("Коэффициент уменьшение угла ")]
        [Category("Параметры угла")]
        [DisplayName("k_decr_ang")]
        public double K_decr_ang
        {
            get { return k_decr_ang; }
            set { k_decr_ang = value; }
        }
        public TrajParams(double _step, int _layers, double _div_step, double _layers_angle, double[] _z, double _h_transf,double _off_z = 0)
        {
            step = _step;
            layers = _layers;
            div_step = _div_step;
            layers_angle = _layers_angle;
            z = _z;
            h_transf = _h_transf;
            off_z = _off_z;
        }
        public TrajParams()
        {

        }
    }
}
