#if !NETSTANDARD1_0

using Simple.BotUtils.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simple.BotUtils.Controllers
{
    public class ControllerManager
    {
        readonly Dictionary<string, EndpointInfo> controllers;
        public bool AcceptSlashInMethodName { get; set; }

        public event EventHandler<FilterEventArgs> Filter;

        public ControllerManager()
        {
            controllers = new Dictionary<string, EndpointInfo>();
        }

        public string[] GetMethodsName()
            => controllers.Keys.ToArray();
        [Obsolete("Use GetMethodsName() instead")]
        public string[] GetMethods()
            => controllers.Keys.ToArray();
        public EndpointInfo[] GetMethodsInfo()
            => controllers.Values.ToArray();

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
                // do not bind "object" methods
                if (method.Name == "Equals") continue;
                if (method.Name == "GetHashCode") continue;
                if (method.Name == "GetType") continue;
                if (method.Name == "MemberwiseClone") continue;
                if (method.Name == "ToString") continue;

                if (method.IsStatic) continue;

                string name = method.Name.ToLower();
                if (name.EndsWith("async")) name = name.Substring(0, name.Length - 5);

                if (method.GetCustomAttributes(false).OfType<IgnoreAttribute>().Any()) continue;

                var nameAttr = method.GetCustomAttributes(false).OfType<MethodNameAttribute>().FirstOrDefault();
                if (nameAttr != null) name = nameAttr.MethodName.ToLower();

                if (!controllers.ContainsKey(name)) controllers.Add(name, new EndpointInfo() { ControllerType = t });

                var ctrl = controllers[name];
                if (ctrl.ControllerType.FullName != t.FullName) throw new InvalidOperationException($"Method {name} is already binded to {ctrl.ControllerType.FullName}. Cannot bind to {t.FullName}.");

                ctrl.Methods.Add(method);
            }
        }

        public void ExecuteFromText(string text)
            => ExecuteFromText<object>(text);
        public T ExecuteFromText<T>(string text)
            => Execute<T>(Startup.ArgumentParser.ArgumentSplit(text));

        public void ExecuteFromText(object context, string text)
            => ExecuteFromText<object>(context, text);
        public T ExecuteFromText<T>(object context, string text)
        {
            // build as:
            // [method] [context] [params]

            var args = Startup.ArgumentParser.ArgumentSplit(text);
            string methodName = args[0];

            object[] newArgs = new object[args.Length - 1 + 1];
            newArgs[0] = context;
            Array.Copy(args, 1, newArgs, 1, args.Length - 1);

            return execute<T>(methodName, newArgs);
        }
        public void Execute(string[] methodWithParameters)
            => Execute<object>(methodWithParameters);
        public T Execute<T>(string[] methodWithParameters)
        {
            if (methodWithParameters.Length == 0) throw new ArgumentException("Must have at least one value");

            if (methodWithParameters.Length == 1)
            {
                return Execute<T>(methodWithParameters[0]);
            }
            else
            {
                string[] arguments = new string[methodWithParameters.Length - 1];
                Array.Copy(methodWithParameters, 1, arguments, 0, arguments.Length);

                string method = methodWithParameters[0];
                return Execute<T>(method, arguments);
            }
        }

        public void Execute(string method, params object[] parameters)
            => Execute<object>(method, parameters);
        public T Execute<T>(string method, params object[] parameters)
            => execute<T>(method, parameters);

        T execute<T>(string method, object[] parameters)
        {
            method = method.ToLower();
            if (method.StartsWith("/") && AcceptSlashInMethodName) method = method.Substring(1);

            if (!controllers.ContainsKey(method)) throw new UnkownMethod(method);
            var info = controllers[method];

            var matchedMethods = info.Methods.Where(m => countParameters(m) == parameters.Length
                                                         || hasParamsArray(m))
                                             .ToArray();
            if (matchedMethods.Length == 0)
            {
                throw new NoSuitableMethodFound(method);
            }

            return execute<T>(info, matchedMethods[0], parameters);
        }

        private bool hasParamsArray(MethodInfo m)
        {
            return m.GetParameters().Any(p => p.GetCustomAttributes(false).Any(a => a is ParamArrayAttribute));
        }
        private int countParameters(MethodInfo m)
        {

            return m.GetParameters().Where(p => !p.GetCustomAttributes(false)
                                                  .Any(a => a is FromDIAttribute))
                                    .Count();
        }

        private T execute<T>(EndpointInfo info, MethodInfo methodInfo, object[] parameters)
        {
            // Get constructors
            var constructors = info.ControllerType.GetConstructors()
                                                  .OrderByDescending(o => o.GetParameters().Length)
                                                  .ToArray();
            if (constructors.Length == 0)
            {
                throw new Exception($"No available constructors for {info.Name}");
            }
            var ctor = constructors[0];
            // And it's parameters
            var ctorParams = ctor.GetParameters();
            // Process parameters from DI
            object[] ctorArgs = new object[ctorParams.Length];
            for (int i = 0; i < ctorParams.Length; i++)
            {
                var type = ctorParams[i].ParameterType;
                ctorArgs[i] = Injector.Get(type);
            }

            // instantiate
            var instance = (IController)ctor.Invoke(ctorArgs);
            // Execute
            var objParams = convertParams(methodInfo, parameters);

            // run filters
            if (Filter != null)
            {
                var ev = new FilterEventArgs()
                {
                    Method = methodInfo.Name,
                    Args = objParams,
                    Attrbiutes = methodInfo.GetCustomAttributes(false).Cast<Attribute>().ToArray(),
                    BlockReason = null,
                };

                foreach (EventHandler<FilterEventArgs> f in Filter.GetInvocationList())
                {
                    f.Invoke(this, ev);
                    if (ev.BlockReason != null)
                    {
                        throw new FilteredException(ev);
                    }
                }
            }

            try
            {
                object result = methodInfo.Invoke(instance, objParams);

                if (methodInfo.ReturnType == typeof(Task))
                {
                    var task = (Task)result;
                    task?.Wait();
                    return default;
                }
                else if (methodInfo.ReturnType.BaseType == typeof(Task) && methodInfo.ReturnType.IsGenericType)
                {
                    var generic = methodInfo.ReturnType.GetGenericArguments()[0];

                    if (generic == typeof(T))
                    {
                        return ((Task<T>)result).Result;
                    }
                    else
                    {
                        var task = (Task)result;
                        task?.Wait();
                        return default;
                    }
                }
                else
                {
                    return (T)result;
                }
            }
            catch (TargetInvocationException ex) { throw ex.InnerException; }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1) throw ex.InnerExceptions[0];
                throw;
            }
            catch { throw; }
        }

        private object[] convertParams(MethodInfo methodInfo, object[] parameters)
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            var paramInfo = methodInfo.GetParameters();
            object[] values = new object[paramInfo.Length];

            int pCount = 0;
            for (int i = 0; i < paramInfo.Length; i++)
            {
                // Execute type translation
                // TODO: refactor existing type conversion

                object p;
                if (paramInfo[i].GetCustomAttributes(false).Any(a => a is FromDIAttribute))
                {
                    var type = paramInfo[i].ParameterType;
                    p = Injector.Get(type);
                }
                else if (paramInfo[i].GetCustomAttributes(false).Any(a => a is ParamArrayAttribute))
                {
                    var arrType = paramInfo[i].ParameterType.GetElementType();
                    if (arrType == typeof(string))
                    {
                        string[] arr = new string[parameters.Length - pCount];
                        //Array.Copy(parameters, pCount, arr, 0, arr.Length);
                        for (int j = 0; j < parameters.Length - pCount; j++)
                        {
                            arr[j] = parameters[pCount + j]?.ToString();
                        }
                        values[i] = arr;
                    }
                    else
                    {
                        object[] arr = new object[parameters.Length - pCount];
                        Array.Copy(parameters, pCount, arr, 0, arr.Length);
                        values[i] = arr;
                    }

                    break; // PARAMS is aways last
                }
                else if (parameters[pCount] == null)
                {
                    // ??  
                    p = parameters[pCount];
                    pCount++;
                }
                else if (parameters[pCount].GetType() == paramInfo[i].ParameterType)
                {
                    // put all remaining args as array
                    p = parameters[pCount];
                    pCount++;
                }
                else if (parameters[pCount] is string)
                {
                    if (paramInfo[i].ParameterType == typeof(string)) p = parameters[pCount]; // do not even try
                    else if (paramInfo[i].ParameterType == typeof(int)) p = Convert.ToInt32(parameters[pCount]);
                    else if (paramInfo[i].ParameterType == typeof(long)) p = Convert.ToInt64(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(double)) p = Convert.ToDouble(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(float)) p = Convert.ToSingle(parameters[pCount], invariant);
                    else if (paramInfo[i].ParameterType == typeof(Guid)) p = Guid.Parse((string)parameters[pCount]);
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
                else
                {
                    throw new InvalidMethodParameterTypeException($"Invalid conversion, see details",
                                                                  paramInfo[i].Name,
                                                                  paramInfo[i].ParameterType,
                                                                  parameters[i].GetType());
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