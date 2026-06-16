using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Security.Cryptography;
using System.Data;
using System.IO;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using Emgu.CV.Util;

namespace opengl3
{
    public class CtSliceFull
    {
        public List<CtSliceInfo>  SlicesAxial { get; set; }
        public List<Mat> SlicesAxial_mat { get; set; }
        public List<Mat> SlicesCoronal { get; set; }
        public List<Mat> SlicesSagital { get; set; }

        public byte[,,] VoxelModel { get; set; }

        public double pix_xy;
        public double pix_z;

        public double axial_koef;
    }

    public class CtSliceInfo
    {
        public string Filename { get; set; }
        public DicomDataset Dataset { get; set; }
        public double[] ImagePositionPatient { get; set; }
        public double[] ImageOrientationPatient { get; set; }
        public double SliceNormalProjection { get; set; } // Значение 'd'
        public Mat Image{ get; set; }

       
    }

    public class DicomSorter
    {

        
        public static CtSliceFull LoadAndSortSlices(string directoryPath)
        {
            var slices = new List<CtSliceInfo>();
            // Загружаем все DICOM файлы из директории
            var dicomFiles = Directory.GetFiles(directoryPath);//*.dcm
            int ind = 0;
            var pix_xy = 0.1;
            var pix_z = 0.1;
            foreach (var file in dicomFiles)
            {

                
                var dicomFile = DicomFile.Open(file);

                if (ind == 0 && dicomFile.Dataset.Contains(DicomTag.PixelSpacing))
                {
                    var pixelSpacing = dicomFile.Dataset.GetValues<double>(DicomTag.PixelSpacing);
                    var pixelSpacingX = pixelSpacing[0];
                    var pixelSpacingY = (pixelSpacing.Length > 1) ? pixelSpacing[1] : pixelSpacing[0];
                    Console.WriteLine("Spacing: " + pixelSpacingX + " " + pixelSpacingY);
                    pix_xy = pixelSpacingX;
                }


                ind++;

                var dataset = dicomFile.Dataset;
                Mat mat = LoadDicomAndConvertToMat(file);
                // Извлекаем необходимые теги
                if (dataset.TryGetValues<double>(DicomTag.ImagePositionPatient, out double[] ipp) &&
                    dataset.TryGetValues<double>(DicomTag.ImageOrientationPatient, out double[] iop))
                {
                    slices.Add(new CtSliceInfo
                    {
                        Filename = file,
                        Dataset = dataset,
                        ImagePositionPatient = ipp,
                        ImageOrientationPatient = iop,
                        Image = mat
                    });
                }
                else
                {
                    Console.WriteLine($"Предупреждение: файл {file} не содержит необходимых тегов пространственного положения.");
                }
            }

            // Вычисляем нормаль к срезу для всех файлов (предполагаем, что все срезы из одной серии имеют одинаковую ориентацию)
            //if (!slices.Any()) return slices;

            // Векторы направления из тега ImageOrientationPatient
            double[] rowDir = { slices[0].ImageOrientationPatient[0], slices[0].ImageOrientationPatient[1], slices[0].ImageOrientationPatient[2] };
            double[] colDir = { slices[0].ImageOrientationPatient[3], slices[0].ImageOrientationPatient[4], slices[0].ImageOrientationPatient[5] };

            // Нормаль = cross(rowDir, colDir)
            double[] normal = CrossProduct(rowDir, colDir);

            // Для каждого среза вычисляем проекцию на нормаль
            foreach (var slice in slices)
            {
                slice.SliceNormalProjection = DotProduct(slice.ImagePositionPatient, normal);
            }

            // Сортируем срезы по проекции
            var sortedSlices = slices.OrderBy(s => s.SliceNormalProjection).ToList();

            // Выводим информацию
            Console.WriteLine($"\nНайдено {sortedSlices.Count} срезов.");
            for (int i = 0; i < sortedSlices.Count; i++)
            {
                Console.WriteLine($"Срез {i}: Файл '{Path.GetFileName(sortedSlices[i].Filename)}', Проекция = {sortedSlices[i].SliceNormalProjection:F2} мм");
                if (i > 0)
                {
                    double spacing = sortedSlices[i].SliceNormalProjection - sortedSlices[i - 1].SliceNormalProjection;
                    Console.WriteLine($"  Расстояние до предыдущего среза: {spacing:F3} мм");
                    pix_z = spacing;
                }
            }
            double size_xy_to_z = pix_z / pix_xy;
            var slices_sec = compCoronalAndSagital(sortedSlices, size_xy_to_z);
            GC.Collect();
            return new CtSliceFull
            {
                SlicesAxial = sortedSlices,
                SlicesCoronal = slices_sec[0],
                SlicesSagital = slices_sec[1],
                SlicesAxial_mat = slices_sec[2],
                pix_xy = pix_xy,
                pix_z = pix_z,
                axial_koef = size_xy_to_z,
                
            };
        }
        public static byte[,,] compVoxelModel(List<CtSliceInfo> cts)
        {
            var data_axial = new List<byte[,]>();
            for (int i = 0; i < cts.Count; i++)
            {
                data_axial.Add((byte[,])cts[i].Image.GetData());
            }

            var data_coronal = new List<Mat>();

            var voxel_model = new byte[data_axial[0].GetLength(0), data_axial[0].GetLength(1),data_axial.Count];

            for (int k = 0; k < data_axial[0].GetLength(0); k++)
            {
                var data_coronal_slice = new byte[data_axial[0].GetLength(0), data_axial.Count];
                for (int i = 0; i < cts.Count; i++)
                {
                    for (int j = 0; j < data_axial[0].GetLength(1); j++)
                    {
                        voxel_model[j,k, i] = data_axial[i][j, k];
                    }
                }

                //data_coronal.Add(ByteArrayToMat(data_coronal_slice));
            }
            GC.Collect();
            return voxel_model;
        }

        

        public static List<List<Mat>> compCoronalAndSagital(List<CtSliceInfo> cts,double size_xy_to_z)
        {
            var axial_mats = new List<Mat>();
            var data_axial = new List<byte[,]>();
            for(int i = 0; i<cts.Count;i++)
            {
                data_axial.Add((byte[,])cts[i].Image.GetData());
                axial_mats.Add(cts[i].Image);
            }
            
            var data_coronal = new List<Mat>();

            for(int k = 0;k< data_axial[0].GetLength(0);k++)
            {
                var data_coronal_slice = new byte[data_axial[0].GetLength(0), data_axial.Count];
                for (int i = 0; i < cts.Count; i++)
                {
                    for (int j = 0; j < data_axial[0].GetLength(1); j++)
                    {
                        data_coronal_slice[j, i] = data_axial[i][j, k];
                    }
                }
                var mat_data = ByteArrayToMat(data_coronal_slice);
                CvInvoke.Resize(mat_data,mat_data,new Size((int)(mat_data.Width * size_xy_to_z), mat_data.Height));
                data_coronal.Add(mat_data);
            }



            var data_sagital = new List<Mat>();

            for (int k = 0; k < data_axial[0].GetLength(0); k++)
            {
                var data_sagital_slice = new byte[data_axial[0].GetLength(1), data_axial.Count];
                for (int i = 0; i < cts.Count; i++)
                {
                    for (int j = 0; j < data_axial[0].GetLength(1); j++)
                    {
                        data_sagital_slice[j, i] = data_axial[i][k, j];
                    }
                }

                var mat_data = ByteArrayToMat(data_sagital_slice);
                CvInvoke.Resize(mat_data, mat_data, new Size((int)(mat_data.Width * size_xy_to_z), mat_data.Height));
                data_sagital.Add(mat_data);

                //data_sagital.Add(ByteArrayToMat(data_sagital_slice));
            }

            //CvInvoke.Imshow("mat_test", mat_test);
            return new List<Mat>[] { data_coronal , data_sagital,axial_mats }.ToList();
        }
        public static Mat ByteArrayToMat(byte[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            int totalBytes = height * width;

            // 1. Преобразуем двумерный массив в одномерный
            byte[] flat = new byte[totalBytes];
            Buffer.BlockCopy(data, 0, flat, 0, totalBytes);

            // 2. Создаём Mat нужного размера и типа (8-бит, 1 канал)
            Mat mat = new Mat(height, width, DepthType.Cv8U, 1);

            // 3. Копируем данные в Mat
            Marshal.Copy(flat, 0, mat.DataPointer, totalBytes);

            return mat;
        }
        public static Mat LoadDicomAndConvertToMat(string dicomFilePath)
        {
            var dicomImage = new DicomImage(dicomFilePath);
            var iImage = dicomImage.RenderImage();
            using (var bitmap = iImage.AsClonedBitmap())
            {
                using (var imgEmgu = bitmap.ToImage<Gray, byte>())
                {
                    return imgEmgu.Mat.Clone();
                }
            }
        }

        private static double DotProduct(double[] a, double[] b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        private static double[] CrossProduct(double[] a, double[] b) => new double[]
        {
        a[1]*b[2] - a[2]*b[1],
        a[2]*b[0] - a[0]*b[2],
        a[0]*b[1] - a[1]*b[0]
        };
    }
    class DicomProcess
    {
       // FellowOakDicom.
       static public void load_dicom(string filePath)
        {

           /* new DicomSetupBuilder()
    .RegisterServices(s => s.AddFellowOakDicom().AddImageManager<WinFormsImageManager>())
    .Build();*/

            // try
            {
                // 1. Синхронная загрузка DICOM-файла
                var dicomFile = DicomFile.Open(filePath);

                // 2. Чтение метаданных (тегов)
                string patientId = dicomFile.Dataset.GetString(DicomTag.PatientID);
                string patientName = dicomFile.Dataset.GetString(DicomTag.PatientName);
                string studyDate = dicomFile.Dataset.GetString(DicomTag.StudyDate);

                dicomFile.Dataset.TryGetValues<double>(DicomTag.ImagePositionPatient, out double[] ipp);
                Console.WriteLine("pos "+ipp[0] + " " + ipp[1] + " " + ipp[2] + " ");

                if (dicomFile.Dataset.Contains(DicomTag.PixelSpacing))
                {
                    var pixelSpacing = dicomFile.Dataset.GetValues<double>(DicomTag.PixelSpacing);
                    var pixelSpacingX = pixelSpacing[0];
                    var pixelSpacingY = (pixelSpacing.Length > 1) ? pixelSpacing[1] : pixelSpacing[0];
                    Console.WriteLine("Spacing: " + pixelSpacingX + " " + pixelSpacingY);
                }

                //Console.WriteLine($"Пациент: {patientName} (ID: {patientId})");
                //Console.WriteLine($"Дата исследования: {studyDate}   ");

                // 3. Доступ к пиксельным данным (изображению)
                var image = new DicomImage(dicomFile.Dataset);


                var bitmap = ConvertDicomToBitmap(image);

                var mat = LoadDicomAndConvertToMat(filePath);


                CvInvoke.Imshow("dicom", mat);
                //Console.WriteLine("asd");

                // Далее полученный фрейм (image.RenderImage()) можно конвертировать в Bitmap или сохранить как PNG
            }
           // catch (Exception ex)
            {
               // Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
        public static byte[,,] compVoxelModel(List<Mat> cts)
        {
            var data_axial = new List<byte[,]>();
            for (int i = 0; i < cts.Count; i++)
            {
                data_axial.Add((byte[,])cts[i].GetData());
            }

            var data_coronal = new List<Mat>();

            var voxel_model = new byte[data_axial[0].GetLength(0), data_axial[0].GetLength(1), data_axial.Count];

            for (int k = 0; k < data_axial[0].GetLength(0); k++)
            {
                var data_coronal_slice = new byte[data_axial[0].GetLength(0), data_axial.Count];
                for (int i = 0; i < cts.Count; i++)
                {
                    for (int j = 0; j < data_axial[0].GetLength(1); j++)
                    {
                        voxel_model[j, k, i] = data_axial[i][j, k];
                    }
                }

                //data_coronal.Add(ByteArrayToMat(data_coronal_slice));
            }

            return voxel_model;
        }

        public static byte[,,] compVoxelModel(List<Mat> cts, Point[] ps)
        {
            var data_axial = new List<byte[,]>();
            for (int i = 0; i < cts.Count; i++)
            {
                data_axial.Add((byte[,])cts[i].GetData());
            }

            var data_coronal = new List<Mat>();

            var x_max = data_axial[0].GetLength(1)-ps[1].X;
            var x_min = data_axial[0].GetLength(1)-ps[1].Y;
            var x_dim = x_max - x_min;
            if (x_min < 0 || x_max >= data_axial[0].GetLength(1) || x_dim < 1) { Console.WriteLine("x_min, x_max, x_dim ,data_axial[0].GetLength(1)" + x_min + " " + x_max + " " + x_dim + " " + cts.Count); return null; }

            var y_max = data_axial[0].GetLength(0) - ps[0].X;
            var y_min = data_axial[0].GetLength(0) - ps[0].Y;
            var y_dim = y_max - y_min;
            if (y_min < 0 || y_max >= data_axial[0].GetLength(0) || y_dim < 1) { Console.WriteLine("y_min, y_max, y_dim ,data_axial[0].GetLength(0)" + y_min + " " + y_max + " " + y_dim + " " + cts.Count); return null; }

            var z_min = ps[2].X;
            var z_max = ps[2].Y;
            var z_dim = z_max - z_min;
            if (z_min < 0 || z_max >= cts.Count || z_dim < 1){ Console.WriteLine("z_min, z_max, z_dim ,cts.Count" + z_min + " " + z_max + " " + z_dim + " "+ cts.Count); return null; }

            var voxel_model = new byte[x_dim, y_dim, z_dim];

            for (int k = 0; k < y_dim; k++)
            {
                for (int i = 0; i < z_dim; i++)
                {
                    for (int j = 0; j < x_dim; j++)
                    {
                        voxel_model[j, k, i] = data_axial[i + z_min][j + x_min, k + y_min];
                    }
                }
            }
            GC.Collect();
            return voxel_model;
        }
        static public Mat filter_bone_ct(Mat mat, int bin_lvl, int gauss_size,int intern = 0)
        {
            var cur_mat = mat.Clone();
            var orig = mat.Clone();
            CvInvoke.CvtColor(orig, orig, ColorConversion.Gray2Bgr);

            CvInvoke.GaussianBlur(cur_mat, cur_mat, new Size(gauss_size, gauss_size), -1);
            CvInvoke.Threshold(cur_mat, cur_mat, bin_lvl, 255, ThresholdType.Binary);

            if(intern == 1) return cur_mat;

            Mat zeroChannel = Mat.Zeros(cur_mat.Size.Height, cur_mat.Size.Width, DepthType.Cv8U, 1);

            // Формируем массив каналов в порядке BGR
            VectorOfMat channels = new VectorOfMat();
            channels.Push(cur_mat);                     // Канал Blue
            channels.Push(cur_mat);              // Канал Green
            channels.Push(zeroChannel);                // Канал Red

            // Выполняем слияние
            Mat result = new Mat();
            CvInvoke.Merge(channels, result);


            return 0.7 * orig + 0.3 * result;
        }

        static public Mat[] filter_bone_cts(List<CtSliceInfo> cts, int bin_lvl, int gauss_size)
        {
            var mats = new List<Mat>();
            for (int i = 0; i < cts.Count; i++)
            {
                mats.Add(filter_bone_ct(cts[i].Image,bin_lvl,gauss_size,1));
            }
            return mats.ToArray();
        }

        static public Bitmap ConvertDicomToBitmap(DicomImage dicomImage)
        {
            // Load the DICOM file
            // Render the image frame (0 is the first frame)
            IImage renderedImage = dicomImage.RenderImage(0);
           
            // Clone into a System.Drawing.Bitmap instance to manage memory cleanly
            Bitmap bitmapImage = renderedImage.As<Bitmap>();
            return bitmapImage;
            
        }

        public static Mat LoadDicomAndConvertToMat(string dicomFilePath)
        {
            var dicomImage = new DicomImage(dicomFilePath);
            var iImage = dicomImage.RenderImage();
            using (var bitmap = iImage.AsClonedBitmap())
            {
                using (var imgEmgu = bitmap.ToImage<Gray, byte>())
                {
                    return imgEmgu.Mat.Clone();
                }
            }
        }



    }
}
