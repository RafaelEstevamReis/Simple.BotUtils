namespace Simple.BotUtils.UnitTests.ControllersTests;

using Simple.BotUtils.Controllers;
using Simple.BotUtils.DI;

public class ControllerManagerTests
{
    private static ControllerManager NewManager()
        => new ControllerManager().AddController<MathController>();

    [Fact]
    public void Execute_returns_method_result()
    {
        var cm = NewManager();
        Assert.Equal("hello", cm.Execute<string>("echo", "hello"));
    }

    [Fact]
    public void Execute_converts_string_parameters()
    {
        var cm = NewManager();
        Assert.Equal(5, cm.Execute<int>("add", "2", "3"));
    }

    [Fact]
    public void Execute_accepts_typed_parameters()
    {
        var cm = NewManager();
        Assert.Equal(5, cm.Execute<int>("add", 2, 3));
    }

    [Fact]
    public void Execute_via_string_array_splits_method_and_args()
    {
        var cm = NewManager();
        Assert.Equal(5, cm.Execute<int>(["add", "2", "3"]));
    }

    [Fact]
    public void ExecuteFromText_splits_and_executes()
    {
        var cm = NewManager();
        Assert.Equal(5, cm.ExecuteFromText<int>("add 2 3"));
    }

    [Fact]
    public void ExecuteFromText_respects_quotes()
    {
        var cm = NewManager();
        Assert.Equal("hello world", cm.ExecuteFromText<string>("echo \"hello world\""));
    }

    [Fact]
    public void ExecuteFromText_with_context_prepends_context_argument()
    {
        var cm = NewManager();
        Assert.Equal("CTX:hi", cm.ExecuteFromText<string>("CTX", "ctx hi"));
    }

    [Fact]
    public void Async_method_name_strips_Async_and_returns_result()
    {
        var cm = NewManager();
        Assert.Equal("hi Bob", cm.Execute<string>("greet", "Bob"));
    }

    [Fact]
    public void Void_method_executes()
    {
        PingController.Pinged = false;
        var cm = new ControllerManager().AddController<PingController>();

        cm.Execute("ping");

        Assert.True(PingController.Pinged);
    }

    [Fact]
    public void Params_array_collects_remaining_arguments()
    {
        var cm = NewManager();
        Assert.Equal("a,b,c", cm.Execute<string>("join", "a", "b", "c"));
    }

    [Fact]
    public void MethodName_attribute_overrides_name()
    {
        var cm = NewManager();
        Assert.Equal("named!", cm.Execute<string>("custom"));
        Assert.Throws<UnkownMethod>(() => cm.Execute<string>("named"));
    }

    [Fact]
    public void Alias_resolves_to_the_method()
    {
        var cm = NewManager();
        Assert.Equal("hello!", cm.Execute<string>("hi"));
        Assert.Equal("hello!", cm.Execute<string>("hey"));
    }

    [Fact]
    public void Ignore_attribute_excludes_method()
    {
        var cm = NewManager();
        Assert.Throws<UnkownMethod>(() => cm.Execute<string>("secret"));
    }

    [Fact]
    public void Unknown_method_throws()
    {
        var cm = NewManager();
        Assert.Throws<UnkownMethod>(() => cm.Execute<string>("nope"));
    }

    [Fact]
    public void Wrong_argument_count_throws_NoSuitableMethodFound()
    {
        var cm = NewManager();
        Assert.Throws<NoSuitableMethodFound>(() => cm.Execute<int>("only"));
    }

    [Fact]
    public void Slash_prefix_requires_AcceptSlashInMethodName()
    {
        var cm = NewManager();
        Assert.Throws<UnkownMethod>(() => cm.Execute<string>("/echo", "hi"));

        cm.AcceptSlashInMethodName = true;
        Assert.Equal("hi", cm.Execute<string>("/echo", "hi"));
    }

    [Fact]
    public void FromDI_parameter_is_injected_and_not_counted()
    {
        Injector.Reset();
        Injector.AddSingleton(new Greeter());
        var cm = new ControllerManager().AddController<DepController>();

        // Only the non-DI "name" argument is supplied.
        Assert.Equal("Hello Ana", cm.Execute<string>("use", "Ana"));
    }

    [Fact]
    public void Filter_receives_method_name_and_args()
    {
        var cm = NewManager();
        FilterEventArgs? captured = null;
        cm.Filter += (_, e) => captured = e;

        cm.Execute<string>("echo", "hi");

        Assert.NotNull(captured);
        Assert.Equal("Echo", captured!.Method);
        Assert.Equal(new object[] { "hi" }, captured.Args);
    }

    [Fact]
    public void Filter_block_reason_throws_FilteredException()
    {
        var cm = NewManager();
        cm.Filter += (_, e) => e.BlockReason = new TestBlock();

        Assert.Throws<FilteredException>(() => cm.Execute<string>("echo", "hi"));
    }

    [Fact]
    public void GetMethodsName_lists_registered_methods()
    {
        var cm = NewManager();
        var names = cm.GetMethodsName();

        Assert.Contains("echo", names);
        Assert.Contains("add", names);
        Assert.DoesNotContain("secret", names);
    }

    // ---- test controllers ----

    private class MathController : IController
    {
        public int Add(int a, int b) => a + b;
        public string Echo(string s) => s;
        public int Only(int a) => a;
        public string Join(params string[] parts) => string.Join(",", parts);
        public string Ctx(object context, string s) => $"{context}:{s}";

        [MethodName("custom")]
        public string Named() => "named!";

        [MethodAlias("hi", "hey")]
        public string Hello() => "hello!";

        [Ignore]
        public string Secret() => "secret";

        public async Task<string> GreetAsync(string name)
        {
            await Task.Yield();
            return "hi " + name;
        }
    }

    private class PingController : IController
    {
        public static bool Pinged;
        public void Ping() => Pinged = true;
    }

    private class DepController : IController
    {
        public string Use([FromDI] Greeter greeter, string name) => greeter.Greet(name);
    }

    private class Greeter
    {
        public string Greet(string name) => "Hello " + name;
    }

    private class TestBlock : FilterException
    {
    }
}
