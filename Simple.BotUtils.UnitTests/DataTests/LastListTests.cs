namespace Simple.BotUtils.UnitTests.DataTests;

using Simple.BotUtils.Data;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "Testing")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2017:Do not use Contains() to check if a value exists in a collection", Justification = "Testing")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "Testing")]
public class LastListTests
{
    [Fact]
    public void Add_under_limit_keeps_insertion_order()
    {
        var list = new LastList<int>(3);
        list.Add(1);
        list.Add(2);

        Assert.Equal(2, list.Count);
        Assert.Equal([1, 2], list);
    }

    [Fact]
    public void Add_past_limit_overwrites_oldest()
    {
        var list = new LastList<int>(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);

        Assert.Equal(3, list.Count);
        Assert.Equal([3, 4, 5], list);
    }

    [Fact]
    public void Indexer_is_oldest_first()
    {
        var list = new LastList<int>(3);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4); // -> 2,3,4

        Assert.Equal(2, list[0]);
        Assert.Equal(3, list[1]);
        Assert.Equal(4, list[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Indexer_out_of_range_throws(int index)
    {
        var list = new LastList<int>(3);
        list.Add(10);
        list.Add(20); // count = 2

        Assert.Throws<ArgumentOutOfRangeException>(() => list[index]);
    }

    [Fact]
    public void ToArray_reflects_logical_order_after_wrap()
    {
        var list = new LastList<int>(3);
        for (int i = 1; i <= 5; i++) list.Add(i); // -> 3,4,5

        Assert.Equal(new[] { 3, 4, 5 }, list.ToArray());
    }

    [Fact]
    public void CopyTo_honors_offset_and_wrap()
    {
        var list = new LastList<int>(3);
        for (int i = 1; i <= 5; i++) list.Add(i); // -> 3,4,5

        var dst = new int[5];
        list.CopyTo(dst, 1);

        Assert.Equal(new[] { 0, 3, 4, 5, 0 }, dst);
    }

    [Fact]
    public void CopyTo_validates_arguments()
    {
        var list = new LastList<int>(3);
        list.Add(1);
        list.Add(2);

        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[2], 1)); // 1 + 2 > 2
    }

    [Fact]
    public void Contains_finds_present_and_rejects_absent()
    {
        var list = new LastList<int>(3);
        for (int i = 1; i <= 4; i++) list.Add(i); // -> 2,3,4

        Assert.True(list.Contains(3));
        Assert.False(list.Contains(1)); // dropped
        Assert.False(list.Contains(99));
    }

    [Fact]
    public void Remove_present_item_shifts_and_reports_true()
    {
        var list = new LastList<int>(3);
        for (int i = 1; i <= 4; i++) list.Add(i); // -> 2,3,4 (internally wrapped)

        Assert.True(list.Remove(3));
        Assert.Equal(new[] { 2, 4 }, list);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Remove_absent_item_returns_false()
    {
        var list = new LastList<int>(3);
        list.Add(1);

        Assert.False(list.Remove(42));
        Assert.Single(list);
    }

    [Fact]
    public void Ring_stays_consistent_after_remove_then_add()
    {
        var list = new LastList<int>(3);
        for (int i = 1; i <= 4; i++) list.Add(i); // -> 2,3,4
        list.Remove(3);                            // -> 2,4
        list.Add(6);                               // -> 2,4,6
        list.Add(7);                               // -> 4,6,7

        Assert.Equal(new[] { 4, 6, 7 }, list);
    }

    [Fact]
    public void AddRange_keeps_only_most_recent()
    {
        var list = new LastList<string>(2);
        list.AddRange(new[] { "a", "b", "c", "d" });

        Assert.Equal(["c", "d"], list);
    }

    [Fact]
    public void AddRange_null_throws()
    {
        var list = new LastList<int>(2);
        Assert.Throws<ArgumentNullException>(() => list.AddRange(null!));
    }

    [Fact]
    public void Clear_empties_the_list()
    {
        var list = new LastList<int>(3);
        list.Add(1);
        list.Add(2);

        list.Clear();

        Assert.Equal(0, list.Count);
        Assert.False(list.Contains(1));
        Assert.Empty(list);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_rejects_non_positive_limit(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LastList<int>(limit));
    }

    [Fact]
    public void Limit_exposes_capacity()
    {
        var list = new LastList<int>(5);
        Assert.Equal(5, list.Limit);
    }

    [Fact]
    public void Enumeration_is_oldest_first()
    {
        var list = new LastList<int>(3);
        list.Add(10);
        list.Add(20);
        list.Add(30);
        list.Add(40); // -> 20,30,40

        var seen = new List<int>();
        foreach (var x in list) seen.Add(x);

        Assert.Equal(new[] { 20, 30, 40 }, seen);
    }

    [Fact]
    public void Modifying_during_enumeration_throws()
    {
        var list = new LastList<int>(4);
        list.Add(1);
        list.Add(2);

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var _ in list) list.Add(9);
        });
    }

    [Fact]
    public void Empty_list_enumerates_to_nothing()
    {
        var list = new LastList<int>(3);
        Assert.Empty(list);
    }

    [Fact]
    public void Exposes_mutable_collection_contract()
    {
        ICollection<int> list = new LastList<int>(3);
        Assert.False(list.IsReadOnly);
    }
}
