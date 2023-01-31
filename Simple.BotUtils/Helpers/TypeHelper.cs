#if !NETSTANDARD1_0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.BotUtils.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Returns all class-types assinable from T in the assembly
        /// </summary>
        public static IEnumerable<Type> GetClassesOfType<T>(Assembly source, Func<Type, bool> filter = null)
        {
            var interfaceType = typeof(T);
            var types = source.GetTypes()
                              .Where(t => t.IsClass)
                              .Where(t => interfaceType.IsAssignableFrom(t));

            if (filter is not null)
            {
                types = types.Where(filter);
            }

            return types;
        }
        /// <summary>
        /// Create T instances from provided types
        /// </summary>
        public static IEnumerable<T> CreateInstancesFor<T>(IEnumerable<Type> types)
        {
            return types.Select(t => (T)Activator.CreateInstance(t));
        }
        /// <summary>
        /// Create T instances from provided types
        /// </summary>
        public static IEnumerable<T> CreateInstancesFor<T>(IEnumerable<Type> types, params object[] args)
        {
            return types.Select(t => (T)Activator.CreateInstance(t, args));
        }
    }
}
#endif