using System;

namespace Simple.BotUtils.Controllers
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromDIAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodNameAttribute : Attribute
    {
        public string MethodName { get; }
        public MethodNameAttribute(string name)
        {
            MethodName = name;
        }

    }
}
