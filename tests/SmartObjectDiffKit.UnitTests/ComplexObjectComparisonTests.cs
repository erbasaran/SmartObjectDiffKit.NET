using FluentAssertions;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class ComplexObjectComparisonTests
{
    [Fact]
    public void Compare_EqualSimpleObjects_ReturnsNoDifferences()
    {
        var a = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };
        var b = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentSimpleObjects_ReturnsDifferences()
    {
        var a = new SimpleObject { Id = 1, Name = "Test", Price = 9.99m };
        var b = new SimpleObject { Id = 2, Name = "Test2", Price = 19.99m };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(3);
    }

    [Fact]
    public void Compare_NestedObjects_DetectsNestedDifferences()
    {
        var a = new Customer
        {
            Id = 1,
            Name = "John",
            Address = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" }
        };
        var b = new Customer
        {
            Id = 1,
            Name = "John",
            Address = new Address { Street = "456 Oak Ave", City = "Springfield", ZipCode = "67890" }
        };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().Contain(d => d.PropertyPath == "Address.Street");
        result.Differences.Should().Contain(d => d.PropertyPath == "Address.ZipCode");
    }

    [Fact]
    public void Compare_NullVsObject_DetectsNullDifference()
    {
        var a = new SimpleObject { Id = 1, Name = "Test" };

        var result = ObjectDiffer.Create().Compare(a, null);
        result.IsEqual.Should().BeFalse();
        result.Differences[0].DifferenceType.Should().Be(DifferenceType.NullChanged);
    }

    [Fact]
    public void Compare_ObjectVsNull_DetectsNullDifference()
    {
        var b = new SimpleObject { Id = 1, Name = "Test" };

        var result = ObjectDiffer.Create().Compare(null, b);
        result.IsEqual.Should().BeFalse();
        result.Differences[0].DifferenceType.Should().Be(DifferenceType.NullChanged);
    }

    [Fact]
    public void Compare_BothNull_ReturnsEqual()
    {
        var result = ObjectDiffer.Create().Compare(null, null);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_SameReference_ReturnsEqual()
    {
        var obj = new SimpleObject { Id = 1, Name = "Test" };
        var result = ObjectDiffer.Create().Compare(obj, obj);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentTypes_DetectsTypeMismatch()
    {
        var a = new Dog { Name = "Rex", Breed = "Labrador" };
        var b = new Cat { Name = "Rex", IsIndoor = true };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.DifferenceType == DifferenceType.TypeChanged);
    }

    [Fact]
    public void Compare_CircularReferences_DoesNotThrow()
    {
        var a1 = new CircularA { Name = "A" };
        var b1 = new CircularB { Name = "B" };
        a1.B = b1;
        b1.A = a1;

        var a2 = new CircularA { Name = "A" };
        var b2 = new CircularB { Name = "B" };
        a2.B = b2;
        b2.A = a2;

        var result = ObjectDiffer.Create().Compare(a1, a2);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DeeplyNestedObjects_RespectsMaxDepth()
    {
        var a = new Customer
        {
            Id = 1,
            Name = "John",
            Orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    Product = "Widget",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Name = "Part A", Quantity = 5 }
                    }
                }
            }
        };
        var b = new Customer
        {
            Id = 1,
            Name = "John",
            Orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    Product = "Widget",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Name = "Part B", Quantity = 5 }
                    }
                }
            }
        };

        var result = ObjectDiffer.Create().MaxDepth(2).Compare(a, b);
        result.MaximumDepth.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public void Compare_DisplayNameAttribute_UsesCustomDisplayNameInPath()
    {
        var a = new DisplayModel { Name = "Old" };
        var b = new DisplayModel { Name = "New" };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.PropertyPath == "Customer Name");
    }
}
