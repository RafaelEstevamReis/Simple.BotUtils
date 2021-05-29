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
