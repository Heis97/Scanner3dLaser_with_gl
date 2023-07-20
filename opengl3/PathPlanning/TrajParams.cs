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
        public int Layers
        {
            get { return layers; }
            set { layers = value; comp_z(); }
        }
        public double step;
        public double Step
        {
            get { return step; }
            set { step = value; }
        }
        public double vel;
        public double Vel
        {
            get { return vel; }
            set { vel = value; }
        }
        public double line_width;
        public double Line_width
        {
            get { return line_width; }
            set { line_width = value; }
        }
        public double off_z;
        public double Off_z
        {
            get { return off_z; }
            set {off_z = value; }
        }
        //------------------------
        public double div_step;
        public double Div_step
        {
            get { return div_step; }
            set { div_step = value; }
        }

        public double ang_x;
        public double Ang_x
        {
            get { return ang_x; }
            set { ang_x  = value; }
        }


        public double layers_angle;
        public double Layers_angle
        {
            get { return layers_angle; }
            set { layers_angle = value; }
        }
        public double h_transf;
        public double H_transf
        {
            get { return h_transf; }
            set { h_transf = value; }
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
