using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
	class Manipulator
	{
		private const float PI = 3.14159265358979f;

		float[] param;
		public float[] flange_matr;
		public Manipulator()
		{
			param = new float[32];

		}
		
		public Vector3d_GL calcPoz(float[] inp)
		{
			for (int i = 0; i < 32; i++)
			{
				param[i] = inp[i];
			}
			float[] A01 = createDHmatrix(param[0], param[1], param[2], param[3]);
			float[] A12 = createDHmatrix(param[4], param[5], param[6], param[7]);
			float[] A23 = createDHmatrix(param[8], param[9], param[10], param[11]);
			float[] A34 = createDHmatrix(param[12], param[13], param[14], param[15]);
			float[] A45 = createDHmatrix(param[16], param[17], param[18], param[19]);
			float[] A56 = createDHmatrix(param[20], param[21], param[22], param[23]);
			float[] A67 = createDHmatrix(param[24], param[25], param[26], param[27]);
			float[] A78 = createDHmatrix(param[28], param[29], param[30], param[31]);
			float[] resMatrix = matrixMultiply(matrixMultiply(matrixMultiply(matrixMultiply(matrixMultiply(matrixMultiply(matrixMultiply(A01, A12), A23), A34), A45), A56), A67), A78);
			flange_matr = resMatrix;
			return new Vector3d_GL(resMatrix[3], resMatrix[7], resMatrix[11]);
		}
		public void printMatrix(float[] matr)
        {
			if(matr.Length == 16)
            {
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						Console.Write(matr [i * 4 + j]+" ");
					}
					Console.WriteLine("");
				}
			}
        }
		float[] createDHmatrix(float Q, float alpha, float a, float d)
		{
			float cosQ = (float)Math.Cos(Q);
			float sinQ = (float)Math.Sin(Q);
			float cosA = (float)Math.Cos(alpha);
			float sinA = (float)Math.Sin(alpha);
			float[] matr = new float[16]
			{
			cosQ,-sinQ*cosA,sinQ*sinA,a*cosQ,
			sinQ, cosQ*cosA,-cosQ*sinA,a*sinQ,
			0   , sinA     , cosA    ,  d,
			0   , 0        , 0       ,  1
			};

			return matr;
		}


		float[] matrixMultiply( float[] m1, float[] m2)
		{
			 
			float[] res = new float[16];
			for (int i = 0; i < 4; ++i)
			{
				for (int j = 0; j < 4; ++j)
				{
					for (int k = 0; k < 4; ++k)
					{
						res[i * 4 + j] += m1[i * 4 + k] * m2[k * 4 + j];
					}
				}
			}
			return res;
		}
		


		public  double[,] GaussJordan(double[,] Matrix)
		{
			int n = Matrix.GetLength(0); //Размерность начальной матрицы

			double[,] xirtaM = new double[n, n]; //Единичная матрица (искомая обратная матрица)
			for (int i = 0; i < n; i++)
				xirtaM[i, i] = 1;

			double[,] Matrix_Big = new double[n, 2 * n]; //Общая матрица, получаемая скреплением Начальной матрицы и единичной
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					Matrix_Big[i, j] = Matrix[i, j];
					Matrix_Big[i, j + n] = xirtaM[i, j];
				}

			//Прямой ход (Зануление нижнего левого угла)
			for (int k = 0; k < n; k++) //k-номер строки
			{
				for (int i = 0; i < 2 * n; i++) //i-номер столбца
					Matrix_Big[k, i] = Matrix_Big[k, i] / Matrix[k, k]; //Деление k-строки на первый член !=0 для преобразования его в единицу
				for (int i = k + 1; i < n; i++) //i-номер следующей строки после k
				{
					double K = Matrix_Big[i, k] / Matrix_Big[k, k]; //Коэффициент
					for (int j = 0; j < 2 * n; j++) //j-номер столбца следующей строки после k
						Matrix_Big[i, j] = Matrix_Big[i, j] - Matrix_Big[k, j] * K; //Зануление элементов матрицы ниже первого члена, преобразованного в единицу
				}
				for (int i = 0; i < n; i++) //Обновление, внесение изменений в начальную матрицу
					for (int j = 0; j < n; j++)
						Matrix[i, j] = Matrix_Big[i, j];
			}

			//Обратный ход (Зануление верхнего правого угла)
			for (int k = n - 1; k > -1; k--) //k-номер строки
			{
				for (int i = 2 * n - 1; i > -1; i--) //i-номер столбца
					Matrix_Big[k, i] = Matrix_Big[k, i] / Matrix[k, k];
				for (int i = k - 1; i > -1; i--) //i-номер следующей строки после k
				{
					double K = Matrix_Big[i, k] / Matrix_Big[k, k];
					for (int j = 2 * n - 1; j > -1; j--) //j-номер столбца следующей строки после k
						Matrix_Big[i, j] = Matrix_Big[i, j] - Matrix_Big[k, j] * K;
				}
			}

			//Отделяем от общей матрицы
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					xirtaM[i, j] = Matrix_Big[i, j + n];

			return xirtaM;
		}
		public double[] Gauss(double[,] Matrix)
		{
			int n = Matrix.GetLength(0); //Размерность начальной матрицы (строки)
			double[,] Matrix_Clone = new double[n, n + 1]; //Матрица-дублер
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n + 1; j++)
					Matrix_Clone[i, j] = Matrix[i, j];

			//Прямой ход (Зануление нижнего левого угла)
			for (int k = 0; k < n; k++) //k-номер строки
			{
				for (int i = 0; i < n + 1; i++) //i-номер столбца
					Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k]; //Деление k-строки на первый член !=0 для преобразования его в единицу
				for (int i = k + 1; i < n; i++) //i-номер следующей строки после k
				{
					double K = Matrix_Clone[i, k] / Matrix_Clone[k, k]; //Коэффициент
					for (int j = 0; j < n + 1; j++) //j-номер столбца следующей строки после k
						Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K; //Зануление элементов матрицы ниже первого члена, преобразованного в единицу
				}
				for (int i = 0; i < n; i++) //Обновление, внесение изменений в начальную матрицу
					for (int j = 0; j < n + 1; j++)
						Matrix[i, j] = Matrix_Clone[i, j];
			}

			//Обратный ход (Зануление верхнего правого угла)
			for (int k = n - 1; k > -1; k--) //k-номер строки
			{
				for (int i = n; i > -1; i--) //i-номер столбца
					Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k];
				for (int i = k - 1; i > -1; i--) //i-номер следующей строки после k
				{
					double K = Matrix_Clone[i, k] / Matrix_Clone[k, k];
					for (int j = n; j > -1; j--) //j-номер столбца следующей строки после k
						Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K;
				}
			}

			//Отделяем от общей матрицы ответы
			double[] Answer = new double[n];
			for (int i = 0; i < n; i++)
				Answer[i] = Matrix_Clone[i, n];

			return Answer;
		}
		void calcRob()
		{
			float[] q = new float[8]{ UtilMatr.toRad(-7.85f),
					UtilMatr.toRad(44.46f),
					UtilMatr.toRad(6.73f),
				   UtilMatr.toRad(-109.17f),
					UtilMatr.toRad(13.97f),
				   UtilMatr.toRad(-23.34f),
					UtilMatr.toRad(59.64f),
					UtilMatr.toRad(0)};
			//pos: 566.31 -30.62 220.70
			//or : 94.43  12.03  132.04
			float dbs = 360.0f;
			float dse = 420.0f;
			float dew = 400.0f;
			float dwf = 126.0f;
			float[] pos = { 0, 0, 0 };
			Manipulator Kuka = new Manipulator();

			float[] par = {  q[0], -PI / 2, 0, dbs,
							 q[1],  PI / 2, 0, 0,
							 q[2],  PI / 2, 0, dse,
							 q[3], -PI / 2, 0, 0,
							 q[4], -PI / 2, 0, dew,
							 q[5], PI / 2, 0, 0,
							 q[6], 0, 0, dwf,
							 0   ,       0, 0, 0 };

			float[] par_1 = {q[0], -PI/2, 0, dbs,
							 q[1], PI / 2, 0, 0,
							 q[2], PI / 2, 0, dse,
							 q[3], -PI / 2, 0, 0,
							 q[4], -PI / 2, 0, dew,
							 q[5], PI / 2, 0, 0,
							 q[6], 0, 0, dwf,
							 0   , 0, 0, 0 };
			Vector3d_GL pos1 = Kuka.calcPoz(par);
			Kuka.printMatrix(Kuka.flange_matr);
			Console.WriteLine("--------------");
			prin.t(UtilMatr.AbcToMatrix(94.43f, 12.03f, 132.04f));
			//print(AbcToMatrix(90f, 90f, 90f));
			Console.WriteLine(pos1);
		}
	};
	
}
