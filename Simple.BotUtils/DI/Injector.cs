using System;
using System.Collections.Generic;

namespace Simple.BotUtils.DI
{
    public class Injector
    {
        static readonly Dictionary<Type, InjectedObject> dicTypes = [];

        public static void AddSingleton(Type t, object instance) => Add(t, instance, null, InjectionType.Singleton);
        public static void AddSingleton<T>(T instance) => Add(typeof(T), instance, null, InjectionType.Singleton);
        public static void AddTransient(Type t, Func<object> constructor) => Add(t, null, constructor, InjectionType.Transient);
        public static void AddTransient<T>(Func<T> constructor) => Add(typeof(T), null, () => constructor, InjectionType.Transient);
        public static void Add(Type t, object? instance, Func<object>? transientConstructor, InjectionType injectionType)
        {
            dicTypes[t] = new InjectedObject()
            {
                Constructor = transientConstructor,
                Instance = instance,
                InjectionType = injectionType,
            };
        }

        public static T Get<T>()
            => (T)Get(typeof(T));
        public static object Get(Type t)
        {
            var info = dicTypes[t];

            if (info.InjectionType == InjectionType.Transient)
            {
                var constructor = info.Constructor ?? throw new InvalidOperationException($"No constructor registered for transient type: {t}");
                return constructor();
            }
            
            if (info.InjectionType == InjectionType.Singleton)
            {
                return info.Instance ?? throw new InvalidOperationException($"No instance registered for singleton type: {t}");
            }

            throw new NotImplementedException();
        }
    }
}
