#if !NETSTANDARD1_0
using System;
using System.IO;
using System.Text;

namespace Simple.BotUtils.Data
{
    public interface IConfigBase
    {
        string FilePath { get; set; }
    }
    public abstract class ConfigBase : IConfigBase
    {
        public string FilePath { get; set; }

        #region Json
        public static T LoadJson<T>(string filePath)
              where T : IConfigBase, new()
            => LoadJson(filePath, new T());
        public static T LoadJson<T>(string filePath, T template)
            where T : IConfigBase
        {
            return JsonSerializer.LoadOrCreate(filePath, template);
        }

        #endregion

        #region XML
        public static T LoadXml<T>(string FilePath)
            where T : IConfigBase, new()
            => LoadXml(FilePath, new T());
        public static T LoadXml<T>(string filePath, T template)
            where T : IConfigBase
        {
            var obj = XmlSerializer.LoadOrCreate(filePath, template);
            obj.FilePath = filePath;
            return obj;
        }
        #endregion
    }
    public static class ConfigExtensions
    {
        public static void Save<T>(this T cfg)
            where T : IConfigBase
            => Save(cfg, cfg.FilePath);

        public static void Save<T>(this T cfg, string filePath)
            where T : IConfigBase
        {
            var fi = new FileInfo(filePath);

            if (fi.Extension.Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                XmlSerializer.ToFile(filePath, cfg);
            }
            else if (fi.Extension.Equals(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                JsonSerializer.ToFile(filePath, cfg);
            }
            else throw new NotSupportedException($"Extension '{fi.Extension}' not supported. Supported extensions: '*.xml', '*.json'");
        }
    }

}
#endif
