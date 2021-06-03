#if !NETSTANDARD1_0
namespace Simple.BotUtils.Data
{
    public interface IConfigBase
    {
#if NET40
        internal string FilePath { get; set; }
#else
        protected internal string FilePath { get; set; }
#endif
    }
    public abstract class ConfigBase : IConfigBase
    {
        string IConfigBase.FilePath { get; set; }

        public static T Load<T>(string FilePath)
            where T : IConfigBase, new()
            => Load<T>(FilePath, new T());
        public static T Load<T>(string FilePath, T template)
            where T : IConfigBase
        {
            var obj = XmlSerializer.LoadOrCreate<T>(FilePath, template);
            obj.FilePath = FilePath;
            return obj;
        }
    }
    public static class ConfigExtensions
    {
        public static void Save<T>(this T cfg)
            where T : IConfigBase
            => Save(cfg, cfg.FilePath);
        public static void Save<T>(this T cfg, string FilePath)
            where T : IConfigBase
            => XmlSerializer.ToFile(FilePath, cfg);
    }
}
#endif
