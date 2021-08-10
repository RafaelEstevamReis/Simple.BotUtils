#if !NETSTANDARD1_0
using System;
using System.IO;

namespace Simple.BotUtils.Data
{
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
