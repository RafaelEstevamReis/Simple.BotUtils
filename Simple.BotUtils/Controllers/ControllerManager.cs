#if !NETSTANDARD1_0

using Simple.BotUtils.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.BotUtils.Controllers
{
    public class ControllerManager
    {
        Dictionary<string, EndpointInfo> controllers;

        public ControllerManager()
        {
            controllers = new Dictionary<string, EndpointInfo>();
        }

        public ControllerManager AddControllers(Assembly assembly)
        {
            var interfaceType = typeof(IController);
            var types = assembly.GetTypes()
                            .Where(t => t.IsClass)
                            .Where(t => interfaceType.IsAssignableFrom(t));

            foreach (var t in types)
            {
                var methods = t.GetMethods();
                addMethods(t, methods);
            }

            return this;
        }
        public ControllerManager AddController<T>()
            where T : IController
        {
            var type = typeof(T);
            addMethods(type, type.GetMethods());

            return this;
        }
        private void addMethods(Type t, MethodInfo[] methods)
        {
            foreach (var method in methods)
            {
                string name = method.Name;

                if (!controllers.ContainsKey(name)) controllers.Add(name, new EndpointInfo() { ControllerType = t });

                var ctrl = controllers[name];
                if (ctrl.ControllerType.FullName != t.FullName) throw new InvalidOperationException($"Method {name} is already binded to {ctrl.ControllerType.FullName}. Cannot bind to {t.FullName}.");

                ctrl.Methods.Add(method);
            }
        }

        public void Execute(string method, params string[] paramters)
            => Execute<object>(method, paramters);
        public T Execute<T>(string method, params string[] parameters)
        {
            if (!controllers.ContainsKey(method)) throw new KeyNotFoundException();

            var info = controllers[method];

            var matchedMethods = info.Methods.Where(m => countParameters(m.GetParameters()) == parameters.Length)
                                             .ToArray();
            if (matchedMethods.Length == 0)
            {
                throw new Exception("No suitable methods found");
            }

            return execute<T>(info, matchedMethods[0], method, parameters);
        }
        private int countParameters(ParameterInfo[] parameterInfos)
        {
            return parameterInfos.Where(p => !p.GetCustomAttributes(false)
                                              .Any(a => a is FromDIAttribute))
                                 .Count();
        }

        private T execute<T>(EndpointInfo info, MethodInfo methodInfo, string method, string[] parameters)
        {
            // instantiate
            IController instance = (IController)Activator.CreateInstance(info.ControllerType);

            var objParams = convertParams(methodInfo, parameters);
            return (T)methodInfo.Invoke(instance, objParams);

        }
        private object[] convertParams(MethodInfo methodInfo, string[] parameters)
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            var paramInfo = methodInfo.GetParameters();
            object[] values = new object[paramInfo.Length];

            int pCount = 0;
            for (int i = 0; i < paramInfo.Length; i++)
            {
                object p;
                if (paramInfo[i].GetCustomAttributes(false).Any(a => a is FromDIAttribute))
                {
                    var type = paramInfo[i].ParameterType;
                    p = Injector.Get(type);
                }
                else
                {
                    if (paramInfo[i].ParameterType == typeof(string)) p = parameters[pCount]; // do not even try
                    else if (paramInfo[i].ParameterType == typeof(int)) p = Convert.ToInt32(parameters[pCount]);
                    else if (paramInfo[i].ParameterType == typeof(long)) p = Convert.ToInt64(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(double)) p = Convert.ToDouble(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(float)) p = Convert.ToSingle(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(Guid)) p = Guid.Parse(parameters[pCount]);
                    else
                    {
                        try
                        {
                            p = Convert.ChangeType(parameters[pCount], paramInfo[i].ParameterType);
                        }
                        catch { p = parameters[pCount]; }
                    }

                    pCount++;
                }
                values[i] = p;
            }

            return values;
        }

        public class EndpointInfo
        {
            public EndpointInfo()
            {
                Methods = new List<MethodInfo>();
            }

            public string Name { get; set; }
            public Type ControllerType { get; set; }
            public List<MethodInfo> Methods { get; set; }
        }

    }
}

#endif