using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CoreTest
{
    public static class JsonOperator
    {
        public static bool SerializeObjectToFile(object value, string fileName)
        {
            return SerializeObjectToFile(value, fileName, string.Empty);
        }

        public static bool SerializeObjectToFile(object value, string fileName, string backupFileName)
        {
            bool ret = false;

            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!string.IsNullOrEmpty(backupFileName) && File.Exists(fileName))
            {
                if (File.Exists(backupFileName))
                    File.Delete(backupFileName);

                File.Move(fileName, backupFileName);
            }

            using (var stream = File.OpenWrite(fileName))
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    string rawString = JsonConvert.SerializeObject(value, Formatting.Indented);
                    writer.Write(rawString);
                    ret = true;
                }
            }

            return ret;
        }

        public static T DeserializeObjectFromFile<T>(string fileName)
        {
            T ret = default(T);

            string rawString = string.Empty;

            if (File.Exists(fileName))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        rawString = reader.ReadToEnd();
                    }
                }

            }

            if (!string.IsNullOrEmpty(rawString))
                ret = JsonConvert.DeserializeObject<T>(rawString);

            return ret;
        }

        public static T DeserializeObjectFromFile<T>(string fileName, string backupFileName, out string sourceFileName)
        {
            T ret = DeserializeObjectFromFile<T>(fileName);
            sourceFileName = fileName;

            if (ret == null && !string.IsNullOrEmpty(backupFileName))
            {
                ret = DeserializeObjectFromFile<T>(backupFileName);
                sourceFileName = backupFileName;
            }

            return ret;
        }
    }
}