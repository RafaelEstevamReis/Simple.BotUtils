#if !NETSTANDARD1_0

namespace Simple.BotUtils.Data
{
    public abstract class ConfigBase<T> : ConfigBase
        where T : IConfigBase, new()
    {
        public static T Load(string filePath)
              => Load(filePath, new T());
        public static T Load(string filePath, T template)
        {
            return Load<T>(filePath, template);
        }
    }

}
#endif
