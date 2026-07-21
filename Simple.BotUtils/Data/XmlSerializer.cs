#if !NETSTANDARD1_0
namespace Simple.BotUtils.Data;

using System.IO;

public static class XmlSerializer
{
    public static T? Load<T>(Stream source)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        return (T?)serializer.Deserialize(source);
    }
    public static void Save<T>(Stream source, T obj)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        serializer.Serialize(source, obj);
        source.Flush();
    }

    public static bool TryLoadFile<T>(string fileName, out T? value)
    {
        value = default;
        if (!File.Exists(fileName)) return false;

        using var fs = new FileStream(fileName, FileMode.Open);
        value = Load<T>(fs);
        return true;
    }
    public static T LoadOrCreate<T>(string fileName, T template)
    {
        if (TryLoadFile(fileName, out T? value)) return value ?? template;

        ToFile(fileName, template);
        return template;
    }
    public static T? FromFile<T>(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open);
        return Load<T>(fs);
    }
    public static void ToFile<T>(string fileName, T obj)
    {
        using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
        Save<T>(fs, obj);
    }
}

#endif
