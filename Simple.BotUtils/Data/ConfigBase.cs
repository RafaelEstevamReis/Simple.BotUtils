#if !NETSTANDARD1_0
namespace Simple.BotUtils.Data
{
    public abstract class ConfigBase
    {
        protected virtual string FilePath { get; set; }

        public void Save<T>()
             where T : ConfigBase, new()
            => SaveTo<T>(FilePath);
        public void SaveTo<T>(string filePath)
              where T : ConfigBase, new()
            => XmlSerializer.ToFile<T>(filePath, (T)this);

        public static T Load<T>(string FilePath)
            where T : ConfigBase, new()
            => Load<T>(FilePath, new T());

        public static T Load<T>(string FilePath, T template)
            where T : ConfigBase
        {
            var obj = XmlSerializer.LoadOrCreate<T>(FilePath, template);
            obj.FilePath = FilePath;
            return obj;
        }
    }
}
#endif
