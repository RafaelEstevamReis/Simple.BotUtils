#if !NETSTANDARD1_0
using System;
using System.IO;

namespace Simple.BotUtils.Data
{
    public abstract class ConfigBase : IConfigBase
    {
        public string FilePath { get; set; }

        protected static T Load<T>(string filePath)
              where T : IConfigBase, new()
            => Load(filePath, new T());
        protected static T Load<T>(string filePath, T template)
            where T : IConfigBase
        {
            var fi = new FileInfo(filePath);

            if (fi.Extension.Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                return LoadXml(filePath, template);
            }
            else if (fi.Extension.Equals(".json", StringComparison.InvariantCultureIgnoreCase))
            {
                return LoadJson(filePath, template);
            }
            else throw new NotSupportedException($"Extension '{fi.Extension}' not supported. Supported extensions: '*.xml', '*.json'");
        }

        #region Json
        protected static T LoadJson<T>(string filePath)
              where T : IConfigBase, new()
            => LoadJson(filePath, new T());
        protected static T LoadJson<T>(string filePath, T template)
            where T : IConfigBase
        {
            var obj = JsonSerializer.LoadOrCreate(filePath, template);
            if(obj != null) obj.FilePath = filePath;
            return obj;
        }

        #endregion

        #region XML
        protected static T LoadXml<T>(string FilePath)
            where T : IConfigBase, new()
            => LoadXml(FilePath, new T());
        protected static T LoadXml<T>(string filePath, T template)
            where T : IConfigBase
        {
            var obj = XmlSerializer.LoadOrCreate(filePath, template);
            obj.FilePath = filePath;
            return obj;
        }
        #endregion
    }
}
#endif
