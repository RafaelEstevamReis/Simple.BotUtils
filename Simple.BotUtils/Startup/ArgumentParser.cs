namespace Simple.BotUtils.Startup
{
    public class ArgumentParser
    {
        public static Arguments Parse(string[] args)
        {
           var collection = new Arguments();

            string last = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    string key = args[i];
                    last = key;
                    collection.Add(key, "");
                }
                else
                {
                    collection[last] = args[i];
                    last = "";
                }
            }

            return collection;
        }
    }
}
