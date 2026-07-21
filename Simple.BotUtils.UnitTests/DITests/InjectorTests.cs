namespace Simple.BotUtils.UnitTests.DITests;

using Simple.BotUtils.DI;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Testing")]
public class InjectorTests
{
    // Injector is global static state; Reset before every test keeps them isolated.
    public InjectorTests() => Injector.Reset();

    [Fact]
    public void AddSingleton_generic_returns_same_instance()
    {
        var instance = new Service();
        Injector.AddSingleton(instance);

        Assert.Same(instance, Injector.Get<Service>());
        Assert.Same(instance, Injector.Get<Service>());
    }

    [Fact]
    public void AddSingleton_byType_returns_same_instance()
    {
        var instance = new Service();
        Injector.AddSingleton(typeof(Service), instance);

        Assert.Same(instance, Injector.Get(typeof(Service)));
    }

    [Fact]
    public void AddSingleton_overwrites_previous_registration()
    {
        var first = new Service();
        var second = new Service();
        Injector.AddSingleton(first);
        Injector.AddSingleton(second);

        Assert.Same(second, Injector.Get<Service>());
    }

    [Fact]
    public void AddTransient_generic_returns_fresh_instance_each_call()
    {
        Injector.AddTransient(() => new Service());

        var a = Injector.Get<Service>();
        var b = Injector.Get<Service>();

        Assert.IsType<Service>(a);
        Assert.NotSame(a, b);
    }

    [Fact]
    public void AddTransient_byType_invokes_constructor_each_call()
    {
        int calls = 0;
        Injector.AddTransient(typeof(Service), () => { calls++; return new Service(); });

        Injector.Get(typeof(Service));
        Injector.Get(typeof(Service));

        Assert.Equal(2, calls);
    }

    [Fact]
    public void Get_unregistered_type_throws()
    {
        Assert.Throws<KeyNotFoundException>(() => Injector.Get<Service>());
    }

    [Fact]
    public void Reset_clears_all_registrations()
    {
        Injector.AddSingleton(new Service());
        Injector.Reset();

        Assert.Throws<KeyNotFoundException>(() => Injector.Get<Service>());
    }

    private class Service
    {
    }
}
