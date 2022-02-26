using System;

namespace Simple.BotUtils.Controllers
{
    public class InvalidMethodParameterTypeException
        : Exception
    {
        public string ParameterName { get; private set; }
        public Type ParameterType { get; private set; }
        public Type GivenType { get; private set; }

        public InvalidMethodParameterTypeException(string message, string parameterName, Type parameterType, Type givenType)
            : base(message)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            GivenType = givenType;
        }
    }
    public class UnkownMethod
            : Exception
    {
        public string MethodName { get; private set; }

        public UnkownMethod(string methodName)
            : base($"No Method {methodName} found.")
        {
            MethodName = methodName;
        }
    }
    public class NoSuitableMethodFound
            : Exception
    {
        public string MethodName { get; private set; }

        public NoSuitableMethodFound(string methodName)
            : base($"No suitable Method {methodName} found with matching parameter count. Special parameters such as [FromDI] are not counted.")
        {
            MethodName = methodName;
        }
    }
    public class FilteredException : Exception
    {
        public string Method { get; }
        public FilterException BlockReason { get; }
        public Attribute[] Attributes { get; }
        public object[] Args { get; }

        public FilteredException(FilterEventArgs filter)
            : base("Method blocked by filter")
        {
            Method = filter.Method;
            BlockReason = filter.BlockReason;
            Attributes = filter.Attrbiutes;
            Args = filter.Args;
        }

    }
    public abstract class FilterException : Exception
    {

    }
}
