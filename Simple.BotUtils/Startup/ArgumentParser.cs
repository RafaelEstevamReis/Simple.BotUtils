using System;
using System.Linq;

namespace Simple.BotUtils.Startup
{
    public class ArgumentParser
    {
        public static Arguments Parse(string[] args)
        {
            var collection = new Arguments();

            string last = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    string key = args[i];
                    last = key;
                    collection.Add(key, "");
                }
                else
                {
                    string val = args[i];
                    if (val.Length > 2)
                    {
                        bool boxed = false;
                        if (val.StartsWith('"') && val.EndsWith('"')) boxed = true;
                        if (val.StartsWith('\'') && val.EndsWith('\'')) boxed = true;
                        if (val.StartsWith('`') && val.EndsWith('`')) boxed = true;

                        if (boxed) val = val.Substring(1, val.Length - 2);
                    }

                    collection[last] = val;
                    last = "";
                }
            }

            return collection;
        }

#if !NETSTANDARD1_0

        public static T ParseAs<T>(string[] args)
            where T : new() => ParseInto<T>(args, new T());

        public static T ParseInto<T>(string[] args, T template)
            where T : new() => MapTo<T>(Parse(args), template);

        public static T MapTo<T>(Arguments arguments, T obj)
            where T : new()
        {
            if (obj == null) obj = new T();

            var type = typeof(T);
            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanRead) continue;
                if (!prop.CanWrite) continue;
                // same name
                var nameOptions = new string[] { $"--{prop.Name}" };

                // ArgumentKey Attributes
                var attrArgKeys = prop.GetCustomAttributes(false)
                                     .OfType<ArgumentKeyAttribute>();
                // Search
                var allKeys = attrArgKeys.SelectMany(o => o.Keys)
                                         .Union(nameOptions);
                foreach (var k in allKeys)
                {
                    if (arguments.Has(k))
                    {
                        setPropValue(prop, arguments[k], obj);
                        break;
                    }
                }
            }
            return obj;
        }
        private static void setPropValue<T>(System.Reflection.PropertyInfo prop, string value, T obj)
        {
            object oValue = value;
            // Not string? try to convert
            if (prop.PropertyType != typeof(string))
            {
                oValue = Convert.ChangeType(oValue, prop.PropertyType);
            }
            prop.SetValue(obj, oValue, index: null);
        }
#endif

    }
}
