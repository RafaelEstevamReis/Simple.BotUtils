namespace Simple.BotUtils.UnitTests.StartupTests;

using Simple.BotUtils.Startup;

public class ArgumentParserTests
{
    // ---- ArgumentSplit ----

    [Fact]
    public void ArgumentSplit_splits_on_whitespace()
    {
        Assert.Equal(new[] { "a", "b", "c" }, ArgumentParser.ArgumentSplit("a b c"));
    }

    [Fact]
    public void ArgumentSplit_keeps_quoted_spaces_and_strips_quotes()
    {
        Assert.Equal(new[] { "hello", "foo bar", "baz" }, ArgumentParser.ArgumentSplit("hello \"foo bar\" baz"));
    }

    [Fact]
    public void ArgumentSplit_empty_string_yields_nothing()
    {
        Assert.Empty(ArgumentParser.ArgumentSplit(""));
    }

    [Fact]
    public void ArgumentSplit_trailing_space_does_not_emit_empty_tail()
    {
        Assert.Equal(new[] { "a" }, ArgumentParser.ArgumentSplit("a "));
    }

    // ---- Parse ----

    [Fact]
    public void Parse_pairs_flags_with_values()
    {
        var args = ArgumentParser.Parse(["-name", "John", "-age", "30"]);

        Assert.Equal("John", args.Get("-name"));
        Assert.Equal("30", args.Get("-age"));
    }

    [Fact]
    public void Parse_flag_without_value_is_empty_string()
    {
        var args = ArgumentParser.Parse(["-verbose"]);

        Assert.True(args.Has("-verbose"));
        Assert.Equal("", args.Get("-verbose"));
    }

    [Fact]
    public void Parse_two_consecutive_flags_keeps_first_empty()
    {
        var args = ArgumentParser.Parse(["-a", "-b", "x"]);

        Assert.Equal("", args.Get("-a"));
        Assert.Equal("x", args.Get("-b"));
    }

    [Fact]
    public void Parse_unboxes_quoted_value()
    {
        var args = ArgumentParser.Parse(["-k", "\"abc\""]);

        Assert.Equal("abc", args.Get("-k"));
    }

    // ---- Arguments ----

    [Fact]
    public void Arguments_Get_returns_null_for_missing_key()
    {
        var args = ArgumentParser.Parse(["-a", "1"]);

        Assert.Null(args.Get("-missing"));
        Assert.False(args.Has("-missing"));
    }

    [Fact]
    public void Arguments_ToNameValue_copies_all_pairs()
    {
        var args = ArgumentParser.Parse(["-a", "1", "-b", "2"]);

        var nvc = args.ToNameValue();

        Assert.Equal("1", nvc["-a"]);
        Assert.Equal("2", nvc["-b"]);
    }

    // ---- MapTo / ParseAs / ParseInto ----

    [Fact]
    public void ParseAs_maps_matching_property_names()
    {
        var model = ArgumentParser.ParseAs<SampleArgs>(["--Name", "John", "--Age", "30"]);

        Assert.Equal("John", model.Name);
        Assert.Equal(30, model.Age);
    }

    [Fact]
    public void MapTo_converts_bool_property()
    {
        var model = ArgumentParser.ParseAs<SampleArgs>(["--Enabled", "true"]);

        Assert.True(model.Enabled);
    }

    [Fact]
    public void MapTo_uses_ArgumentKey_aliases()
    {
        var model = ArgumentParser.ParseAs<SampleArgs>(["-n", "Maria"]);

        Assert.Equal("Maria", model.Name);
    }

    [Fact]
    public void ParseInto_keeps_untouched_defaults()
    {
        var template = new SampleArgs { Name = "default", Age = 99 };

        var model = ArgumentParser.ParseInto(["--Age", "5"], template);

        Assert.Equal("default", model.Name); // not supplied -> keeps template value
        Assert.Equal(5, model.Age);
    }

    private class SampleArgs
    {
        [ArgumentKey("-n", "--nome")]
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public bool Enabled { get; set; }
    }
}
