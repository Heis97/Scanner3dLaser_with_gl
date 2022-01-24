using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    class Magnetic
    {
        float PI = 3.1415926535f;
        float[] meshForce(float[] meshB, float[] meshGradB)
        {
            var force = new float[meshGradB.Length];
            Console.WriteLine(meshB.Length);
            Console.WriteLine(meshGradB.Length);
            for (int i = 0; i < meshGradB.Length; i += 3)
            {
                force[i] = meshGradB[i];
                force[i + 1] = meshGradB[i + 1];
                force[i + 2] = calcF(meshB[i + 2], meshGradB[i + 2]);
            }
            return force;
        }
        float calcF(float B, float gradB)
        {
            var r = 4 * Math.Pow(10, -6);
            var u0 = 1.2566 * Math.Pow(10, -6);
            var up = 0.999992;
            var uf = 1.000012;
            var K = (up - uf) / (up + 2 * uf);
            var f = 2 * PI * Math.Pow(r, 3) * uf;


            var v_bak = VolumeSph(r);

            f = v_bak * uf * K / (u0);
            var ro_bak = 1.5;
            var ro_liq = 1.015;
            Console.WriteLine("F = " + f);
            return (float)f;
        }


        double VolumeSph(double r)
        {
            return PI * (4 / 3) * Math.Pow(r, 3);
        }
        double calcFarch_Gr(double V_b, double ro_liq, double ro_b)
        {
            double g = 9.8;
            var m = V_b * ro_b;
            var Fa = ro_liq * g * V_b;
            var Fg = m * g;
            return Fa - Fg;

        }
        float calcGradZ(float x1, float y1, float z1, float dx, float z2, float dy, float z3)
        {
            float mash = 0.001f;
            return (float)Math.Sqrt((z1 - z3 / mash * dx) * (z1 - z3 / mash * dx) + (z1 - z3 / mash * dy) * (z1 - z3 / mash * dy));
        }
        float[] cutMeshLvl(float[] mesh, float Lvl)
        {
            var meshL = new float[mesh.Length];
            for (int i = 0; i < meshL.Length; i += 3)
            {
                Console.WriteLine(mesh[i + 2] + " " + Lvl);
                if (-mesh[i + 2] >= Lvl)
                {
                    meshL[i] = mesh[i];
                    meshL[i + 1] = mesh[i + 1];
                    meshL[i + 2] = mesh[i + 2];
                }
            }
            return meshL;
        }



        Image<Gray, float> gradMesh(float[] mesh, int cols = 41, int rows = 91)
        {
            var grad = new float[mesh.Length];
            var im_grad = new Image<Gray, float>(cols, rows);
            var im_grad_data = new float[cols, rows, 1];
            for (int i = 0; i < cols - 1; i++)
            {
                for (int j = 0; j < rows - 1; j++)
                {
                    // Console.WriteLine(3 * (i * rows + j) + " " + i + " " + j);
                    var x1 = mesh[3 * (i * rows + j)];
                    var y1 = mesh[3 * (i * rows + j) + 1];
                    var z1 = mesh[3 * (i * rows + j) + 2];
                    var z2 = mesh[3 * ((i + 1) * rows + j) + 2];
                    var z3 = mesh[3 * (i * rows + j + 1) + 2];

                    grad[3 * (i * rows + j)] = mesh[3 * (i * rows + j)];
                    grad[3 * (i * rows + j) + 1] = mesh[3 * (i * rows + j) + 1];
                    grad[3 * (i * rows + j) + 2] = calcGradZ(x1, y1, z1, 0.5f, z2, 0.2f, z3);
                    im_grad_data[i, j, 0] = grad[3 * (i * rows + j) + 2];
                }
            }
            return new Image<Gray, float>(im_grad_data);
        }
    }
}
