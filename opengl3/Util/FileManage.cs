﻿using Newtonsoft.Json;
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
        public static double[] loadFronJson(string path)
        {
            string jsontext = "";
            using (StreamReader file = File.OpenText(path))
            {
                jsontext = file.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<double[]>(jsontext);
        }
    }
}
