using FluentAssertions;
using SmartObjectDiffKit.Configuration;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class ConfigurationTests
{
    [Fact]
    public void IgnoreProperty_ByName_IgnoresProperty()
    {
        var a = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };
        var b = new SimpleObject { Id = 2, Name = "Test", Price = 9.99m };

        var result = ObjectDiffer.Create()
            .IgnoreProperty("Id")
            .Compare(a, b);

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void IgnoreProperty_ByExpression_IgnoresProperty()
    {
        var a = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };
        var b = new SimpleObject { Id = 2, Name = "Test", Price = 9.99m };

        var result = ObjectDiffer.Create()
            .IgnoreProperty<SimpleObject>(x => x.Id)
            .Compare(a, b);

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void IgnoreProperty_ByAttribute_IgnoresProperty()
    {
        var a = new IgnoredPropertyModel { Id = 1, Name = "Test", LastModified = DateTime.Now };
        var b = new IgnoredPropertyModel { Id = 1, Name = "Test", LastModified = DateTime.Now.AddDays(1) };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void CaseInsensitive_CompareStrings_IgnoresCase()
    {
        var result = ObjectDiffer.Create()
            .CaseInsensitive()
            .Compare("Hello", "hello");

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void TrimStrings_CompareStrings_TrimsBeforeCompare()
    {
        var result = ObjectDiffer.Create()
            .TrimStrings()
            .Compare("  hello  ", "hello");

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void MaxDepth_LimitsDepth()
    {
        var a = new Customer
        {
            Id = 1,
            Name = "John",
            Orders = new List<Order>
            {
                new Order { Id = 1, Product = "A", Items = new List<OrderItem> { new() { Name = "X" } } }
            }
        };
        var b = new Customer
        {
            Id = 1,
            Name = "John",
            Orders = new List<Order>
            {
                new Order { Id = 1, Product = "B", Items = new List<OrderItem> { new() { Name = "Y" } } }
            }
        };

        var result = ObjectDiffer.Create()
            .MaxDepth(2)
            .Compare(a, b);

        result.MaximumDepth.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void MaxDepth_ThrowsOnZero()
    {
        var act = () => ObjectDiffer.Create().MaxDepth(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IgnoreProperty_ByPredicate_IgnoresMatchingProperties()
    {
        var a = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };
        var b = new SimpleObject { Id = 2, Name = "Test", Price = 19.99m };

        var result = ObjectDiffer.Create()
            .IgnoreProperty((name, type) => name == "Id" || name == "Price")
            .Compare(a, b);

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void CustomComparer_UsesCustomLogic()
    {
        var comparer = new AlwaysEqualComparer();
        var result = ObjectDiffer.Create()
            .UseComparer<string>(comparer)
            .Compare("hello", "world");

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void WithDefaultSeverity_SetsSeverity()
    {
        var a = new SimpleObject { Id = 1, Name = "A" };
        var b = new SimpleObject { Id = 2, Name = "B" };

        var result = ObjectDiffer.Create()
            .WithDefaultSeverity(DifferenceSeverity.Critical)
            .Compare(a, b);

        result.Differences.Should().AllSatisfy(d =>
            d.Severity.Should().Be(DifferenceSeverity.Critical));
    }

    private class AlwaysEqualComparer : ICustomComparer
    {
        public bool AreEqual(object? x, object? y) => true;
    }
}
