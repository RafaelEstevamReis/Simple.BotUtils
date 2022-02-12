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
}
