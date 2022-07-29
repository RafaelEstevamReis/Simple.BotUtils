using Simple.BotUtils.Controllers;
using System;
using System.Threading.Tasks;

namespace Simple.BotUtils.Test.ControllerSample
{
    public class MyControllers : IController
    {
        public void ShowInfo(string info) => Console.WriteLine(info);
        public void ShowNumber(int number) => Console.WriteLine(number);
        public void ShowDouble(double number) => Console.WriteLine(number);
        public int Sum(int a, int b) => a + b;
        public void ShowMyName([FromDI] MyConfig cfg) => Console.WriteLine(cfg.MyName);

        public void ShowCallerInfo(int contextParam, string textParams, [FromDI] MyConfig cfg)
            => Console.WriteLine($"ShowCallerInfo[{contextParam}] {textParams}");

        public void Echo(params string[] contents)
        {
            Console.WriteLine(string.Join(' ', contents));
        }
    }
    public class AdvancedControllers : IController
    {
        private readonly MyConfig config;

        public AdvancedControllers(MyConfig config)
        {
            this.config = config;
        }

        [MethodName("ShowMyName")]
        public void ShowMyName2() => Console.WriteLine(config.MyName);

        [MethodAlias("little_1", "little_2", "little_3")]
        [MethodAlias("big_1", "big_2", "big_3")]
        public void MultipleNames() => Console.WriteLine("MultipleNames");

        public async Task<string> DoTaskAsync()
        {
            Console.WriteLine("Async");
            //throw new Exception("test");
            return await Task.Run(() => "Async");
        }

        public void SelfAware(ControllerManager.EndpointInfo endpoint, System.Reflection.MethodInfo method) 
            => Console.WriteLine($"Endpoint Name: {endpoint.Name} | MethodName: {method.Name}");

        [Ignore]
        public void ThisMethodIsNotAccessible() { }
        public static void StaticMethodsAreNotAvailable() { }
    }
}
