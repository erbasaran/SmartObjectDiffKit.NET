using FluentAssertions;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class SpecialTypeComparisonTests
{
    [Fact]
    public void Compare_EqualRecords_ReturnsNoDifferences()
    {
        var a = new PersonRecord("John", 30);
        var b = new PersonRecord("John", 30);

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentRecords_ReturnsDifferences()
    {
        var a = new PersonRecord("John", 30);
        var b = new PersonRecord("Jane", 25);

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(2);
    }

    [Fact]
    public void Compare_EqualStructs_ReturnsNoDifferences()
    {
        var a = new PointStruct { X = 1.0, Y = 2.0 };
        var b = new PointStruct { X = 1.0, Y = 2.0 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentStructs_ReturnsDifferences()
    {
        var a = new PointStruct { X = 1.0, Y = 2.0 };
        var b = new PointStruct { X = 3.0, Y = 4.0 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_Tuples_EqualTuples_ReturnsNoDifferences()
    {
        var a = (1, "hello", true);
        var b = (1, "hello", true);

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_Tuples_DifferentTuples_ReturnsDifferences()
    {
        var a = new { Item1 = 1, Item2 = "hello", Item3 = true };
        var b = new { Item1 = 2, Item2 = "world", Item3 = false };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_ValueTuples_EqualTuples_ReturnsNoDifferences()
    {
        var a = ValueTuple.Create(1, "hello");
        var b = ValueTuple.Create(1, "hello");

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_AnonymousTypes_Equal_ReturnsNoDifferences()
    {
        var a = new { Name = "John", Age = 30 };
        var b = new { Name = "John", Age = 30 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_AnonymousTypes_Different_ReturnsDifferences()
    {
        var a = new { Name = "John", Age = 30 };
        var b = new { Name = "Jane", Age = 25 };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_NullableInts_BothNull_ReturnsEqual()
    {
        int? a = null;
        int? b = null;

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_NullableInts_OneNull_ReturnsNotEqual()
    {
        int? a = 42;
        int? b = null;

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_NullableInts_BothHaveValue_Equal_ReturnsEqual()
    {
        int? a = 42;
        int? b = 42;

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_InheritedProperties_DetectsDifferences()
    {
        var a = new Dog { Name = "Rex", Breed = "Labrador" };
        var b = new Dog { Name = "Rex", Breed = "Poodle" };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().ContainSingle(d => d.PropertyPath == "Breed");
    }

    [Fact]
    public void Compare_JaggedArrays_Equal_ReturnsEqual()
    {
        var a = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
        var b = new[] { new[] { 1, 2 }, new[] { 3, 4 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_JaggedArrays_Different_ReturnsNotEqual()
    {
        var a = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
        var b = new[] { new[] { 1, 2 }, new[] { 3, 5 } };

        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeFalse();
    }
}
