using System;
using System.Collections.Generic;

namespace Simple.BotUtils.DI
{
    public class Injector
    {
        static Dictionary<Type, InjectedObject> dicTypes;
        static Injector()
        {
            dicTypes = new Dictionary<Type, InjectedObject>();
        }

        public static void AddSingleton(Type t, object instance) => Add(t, instance, null, InjectionType.Singleton);
        public static void AddTransient(Type t, Func<object> constructor) => Add(t, null, constructor, InjectionType.Transient);
        public static void Add(Type t, object instance, Func<object> transientConstructor, InjectionType injectionType)
        {
            dicTypes[t] = new InjectedObject()
            {
                Constructor = transientConstructor,
                Instance = instance,
                InjectionType = injectionType,
            };
        }

        public static T Get<T>()
        {
            var info = dicTypes[typeof(T)];
            object obj;

            if (info.InjectionType == InjectionType.Transient)
            {
                obj = info.Constructor(); 
            }
            else if (info.InjectionType == InjectionType.Singleton)
            {
                obj = info.Instance;
            }
            else
            {
                throw new NotImplementedException();
            }

            return (T)obj;
        }
    }
}
