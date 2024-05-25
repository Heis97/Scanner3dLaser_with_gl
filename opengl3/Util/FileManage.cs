using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    static public  class FileManage
    {
        public static void saveToJson(List<RozumPoint> list, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, list);
            }
        }

        public static void saveToJson(Dictionary<string,RozumPoint> list, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, list);
            }
        }

        public static void saveToJson(List<Dictionary<string, RozumPoint>> list, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, list);
            }
        }
        public static List<Dictionary<string, RozumPoint>> loadFromJson(string path)
        {
            string jsontext = "";
            using (StreamReader file = File.OpenText(path))
            {
                jsontext = file.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<List<Dictionary<string, RozumPoint>>>(jsontext);
        }
            
        public static void saveToJson_flats(Flat3d_GL[] list, string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, list);
            }
        }
        public static Flat3d_GL[] loadFromJson_flats(string path)
        {
            string jsontext = "";
            using (StreamReader file = File.OpenText(path))
            {
                jsontext = file.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<Flat3d_GL[]>(jsontext);
        }
    }
}
