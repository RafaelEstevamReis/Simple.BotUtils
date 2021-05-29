#if !NETSTANDARD1_1
using System.IO;

namespace Simple.BotUtils.Data
{
    public static class XmlSerializer
    {
        public static T Load<T>(Stream source)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(source);
        }
        public static void Save<T>(Stream source, T obj)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            serializer.Serialize(source, obj);
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
