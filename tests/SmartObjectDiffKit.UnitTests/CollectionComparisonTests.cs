using FluentAssertions;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class CollectionComparisonTests
{
    [Fact]
    public void Compare_EqualLists_ReturnsNoDifferences()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2, 3 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentLists_DetectsChanges()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2, 4 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_ListWithExtraItems_DetectsAdded()
    {
        var a = new List<int> { 1, 2 };
        var b = new List<int> { 1, 2, 3 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.DifferenceType == DifferenceType.Added);
    }

    [Fact]
    public void Compare_ListWithMissingItems_DetectsRemoved()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.DifferenceType == DifferenceType.Removed);
    }

    [Fact]
    public void Compare_Arrays_EqualArrays_ReturnsNoDifferences()
    {
        var a = new[] { 1, 2, 3 };
        var b = new[] { 1, 2, 3 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_Arrays_DifferentArrays_DetectsChanges()
    {
        var a = new[] { 1, 2, 3 };
        var b = new[] { 1, 2, 4 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualDictionaries_ReturnsNoDifferences()
    {
        var a = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var b = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentDictionaries_DetectsChanges()
    {
        var a = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var b = new Dictionary<string, int> { { "a", 1 }, { "b", 3 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_DictionaryWithExtraKey_DetectsAdded()
    {
        var a = new Dictionary<string, int> { { "a", 1 } };
        var b = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.DifferenceType == DifferenceType.Added);
    }

    [Fact]
    public void Compare_DictionaryWithMissingKey_DetectsRemoved()
    {
        var a = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var b = new Dictionary<string, int> { { "a", 1 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.DifferenceType == DifferenceType.Removed);
    }

    [Fact]
    public void Compare_IgnoreCollectionOrder_SameItemsDifferentOrder_ReturnsEqual()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 3, 1, 2 };

        var result = ObjectDiffer.Create().IgnoreCollectionOrder().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_OrderSensitive_DifferentOrder_ReturnsNotEqual()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 3, 1, 2 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EmptyLists_ReturnsEqual()
    {
        var a = new List<int>();
        var b = new List<int>();

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_HashSets_EqualSets_ReturnsEqual()
    {
        var a = new HashSet<string> { "a", "b", "c" };
        var b = new HashSet<string> { "a", "b", "c" };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_KeyedCollections_MatchesByKey()
    {
        var a = new List<KeyedItem>
        {
            new() { Id = "key1", Value = "Original" },
            new() { Id = "key2", Value = "Unchanged" }
        };
        var b = new List<KeyedItem>
        {
            new() { Id = "key2", Value = "Unchanged" },
            new() { Id = "key1", Value = "Modified" }
        };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(1);
        result.Differences[0].PropertyPath.Should().Be("[\"key1\"].Value");
        result.Differences[0].OldValue.Should().Be("Original");
        result.Differences[0].NewValue.Should().Be("Modified");
    }

    [Fact]
    public void Compare_KeyedCollections_DetectsAddedAndRemovedByKey()
    {
        var a = new List<KeyedItem>
        {
            new() { Id = "key1", Value = "Val" }
        };
        var b = new List<KeyedItem>
        {
            new() { Id = "key2", Value = "Val" }
        };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(2);
        result.Differences.Should().Contain(d => d.DifferenceType == DifferenceType.Removed && d.PropertyPath == "[0]");
        result.Differences.Should().Contain(d => d.DifferenceType == DifferenceType.Added && d.PropertyPath == "[\"key2\"]");
    }

    [Fact]
    public void Compare_UnorderedComplexObjects_MatchesStructurally()
    {
        var a = new List<OrderItem>
        {
            new() { Name = "Widget", Quantity = 5 },
            new() { Name = "Gadget", Quantity = 2 }
        };
        var b = new List<OrderItem>
        {
            new() { Name = "Gadget", Quantity = 2 },
            new() { Name = "Widget", Quantity = 5 }
        };

        var result = ObjectDiffer.Create().IgnoreCollectionOrder().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }
}
