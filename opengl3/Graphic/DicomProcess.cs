using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FellowOakDicom;
using FellowOakDicom.Imaging;

namespace opengl3
{
    class DicomProcess
    {
       // FellowOakDicom.
       static public void load_dicom(string filePath)
        {

           // try
            {
                // 1. Синхронная загрузка DICOM-файла
                var dicomFile = DicomFile.Open(filePath);

                // 2. Чтение метаданных (тегов)
                string patientId = dicomFile.Dataset.GetString(DicomTag.PatientID);
                string patientName = dicomFile.Dataset.GetString(DicomTag.PatientName);
                string studyDate = dicomFile.Dataset.GetString(DicomTag.StudyDate);

                

                Console.WriteLine($"Пациент: {patientName} (ID: {patientId})");
                Console.WriteLine($"Дата исследования: {studyDate}   "+ dicomFile.Dataset.GetString(DicomTag.ApexPosition));

                // 3. Доступ к пиксельным данным (изображению)
                var image = new DicomImage(dicomFile.Dataset);
                // Далее полученный фрейм (image.RenderImage()) можно конвертировать в Bitmap или сохранить как PNG
            }
           // catch (Exception ex)
            {
               // Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
    }
}
