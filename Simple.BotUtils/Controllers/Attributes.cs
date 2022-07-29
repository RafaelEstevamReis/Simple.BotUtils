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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MethodAliasAttribute : Attribute
    {
        public string[] Alisases { get; }
        public MethodAliasAttribute(params string[] alisases)
        {
            Alisases = alisases;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    { }
}
