using Simple.BotUtils.Data;

namespace Simple.BotUtils.Test
{
    public class MyConfig : ConfigBase<MyConfig>
    {
        [Startup.ArgumentKey("-n", "--name")]
        public string MyName { get; set; }
        [Startup.ArgumentKey("-i", "--info")]
        public string MyInfo { get; set; }
        [Startup.ArgumentKey("-nb", "--number")]
        public int MyNumber { get; set; }
        [Startup.ArgumentKey("-ln", "--long")]
        public long MyLongNumber { get; set; }
        [Startup.ArgumentKey("-nn", "--MyNegativeNumber")]
        public int MyNegativeNumber { get; set; }

        public override string ToString()
        {
            return $"{MyName} | {MyInfo} | {MyNumber}: {MyLongNumber}";
        }
    }
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
