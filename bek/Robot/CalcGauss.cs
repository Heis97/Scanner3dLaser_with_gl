using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
	public class CalcGauss
	{

		double[,] fkt = null;    //Fkt.-Array
		double[] knt = null;         //Konstanten-Array
		double[] erg = null;         //Ergebnis-Array

		int j;                  // Zeilen
		int k2;                 // Spalten

		double result;
		public CalcGauss(double[,] matr, double[] col)
        {
			if(matr != null && col !=null)
            {
				if (matr.GetLength(0) == col.Length && matr.GetLength(0) == matr.Length/ matr.GetLength(0))
				{
					int max = col.Length-1;
					fkt = matr;
					knt = col;
					erg = new double[col.Length];
					j = max;
					k2 = max;
					CalcGls();
				}
			}
			
		}

		double[,] transp(double[,] matr)
        {
			if(matr.GetLength(0)== matr.GetLength(1))
            {
				int n = matr.GetLength(0);
				var tr_matr = new double[n, n];
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < n; j++)
					{
						tr_matr[i, j] = matr[j, i];
					}
				}
				return tr_matr;
			}
			else
            {
				return null;
            }
			
        }
		public double[] getAnswer()
        {
			return erg;
        }
		public void CGls(int s)
		{
			int z;
			double fakt;
			int id;
			z = s + 1;
			id = 0;
			
			for (z = s + 1; z <= j; z++)
			{				
				if (fkt[z, s] != 0)
				{
					fakt = (-1 * fkt[z, s]) / fkt[s, s];
					for (id = 0; id <= k2; id++)
					{
						fkt[z, id] = fkt[z, id] + (fkt[s, id] * fakt);
					}
					knt[z] = knt[z] + (knt[s] * fakt);
				}
			}
		}

		public void CFkt()
		{
			int z;
			int id;
			double sum;


			for (z = j; z >= 0; z--)
			{
				sum = 0;
				for (id = 0; id <= k2; id++)
				{
					sum = sum + (erg[id] * fkt[z, id]);
				}
				erg[z] = (knt[z] + sum * -1) / fkt[z, z];
				result = erg[z];
			}
		}

		public void CalcGls()
		{
			int i;

			for (i = 0; i <= k2; i++)
			{
				CGls(i);
			}
			CFkt();
		}


	} //Ende Klasse CalcGLSTyp
}
