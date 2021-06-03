#if !(NETSTANDARD1_0 || NET40 || NET45)
using System.IO;

namespace Simple.BotUtils.Data
{
    /// <summary>
    /// Basic json serializer/deserializer based on System.Text.Json
    /// </summary>
    public class JsonSerializer
    {
        public static T Load<T>(Stream source)
        {
            var sr = new StreamReader(source);
            return System.Text.Json.JsonSerializer.Deserialize<T>(sr.ReadToEnd());
        }
        public static void Save<T>(Stream source, T obj)
        {
            var sr = new StreamWriter(source);
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            sr.Write(json);
        }

        public static bool TryLoadFile<T>(string fileName, out T value)
        {
            value = default;
            if (!File.Exists(fileName)) return false;

            using FileStream fs = new FileStream(fileName, FileMode.Open);
            value = Load<T>(fs);
            return true;
        }
        public static T LoadOrCreate<T>(string fileName, T template)
        {
            if (TryLoadFile(fileName, out T value)) return value;

            ToFile(fileName, template);
            return template;
        }
        public static T FromFile<T>(string fileName)
        {
            using FileStream fs = new FileStream(fileName, FileMode.Open);
            return Load<T>(fs);
        }
        public static void ToFile<T>(string fileName, T obj)
        {
            using FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            Save<T>(fs, obj);
        }
    }
}
#endif