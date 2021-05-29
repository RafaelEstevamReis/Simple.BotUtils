#if !NETSTANDARD1_1
namespace Simple.BotUtils.Data
{
    public abstract class ConfigBase
    {
        public void Save(string filePath) => XmlSerializer.ToFile(filePath, this);

        public static T Load<T>(string FilePath)
            where T : ConfigBase
        {
            return XmlSerializer.FromFile<T>(FilePath);
        }
        public static T LoadOrCreate<T>(string FilePath)
            where T : ConfigBase, new()
        {
            return XmlSerializer.LoadOrCreate<T>(FilePath, new T());
        }
        public static T LoadOrCreate<T>(string FilePath, T template) 
            where T : ConfigBase
        {
            return XmlSerializer.LoadOrCreate<T>(FilePath, template);
        }
    }
}
#endif
