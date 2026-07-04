using FluentAssertions;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class PrimitiveComparisonTests
{
    [Fact]
    public void Compare_EqualIntegers_ReturnsNoDifferences()
    {
        var result = ObjectDiffer.Create().Compare(42, 42);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentIntegers_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(42, 99);
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(1);
        result.Differences[0].DifferenceType.Should().Be(DifferenceType.Modified);
    }

    [Fact]
    public void Compare_EqualStrings_ReturnsNoDifferences()
    {
        var result = ObjectDiffer.Create().Compare("hello", "hello");
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentStrings_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare("hello", "world");
        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().Be(1);
    }

    [Fact]
    public void Compare_EqualBools_ReturnsNoDifferences()
    {
        var result = ObjectDiffer.Create().Compare(true, true);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentBools_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(true, false);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualDecimals_ReturnsNoDifferences()
    {
        var result = ObjectDiffer.Create().Compare(3.14m, 3.14m);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentDecimals_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(3.14m, 2.72m);
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualGuids_ReturnsNoDifferences()
    {
        var guid = Guid.NewGuid();
        var result = ObjectDiffer.Create().Compare(guid, guid);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentGuids_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(Guid.NewGuid(), Guid.NewGuid());
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualDateTimes_ReturnsNoDifferences()
    {
        var dt = new DateTime(2024, 1, 1, 12, 0, 0);
        var result = ObjectDiffer.Create().Compare(dt, dt);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentDateTimes_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 6, 15));
        result.IsEqual.Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualEnums_ReturnsNoDifferences()
    {
        var result = ObjectDiffer.Create().Compare(Color.Red, Color.Red);
        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentEnums_ReturnsOneDifference()
    {
        var result = ObjectDiffer.Create().Compare(Color.Red, Color.Blue);
        result.IsEqual.Should().BeFalse();
    }

    [Theory]
    [InlineData((byte)1, (byte)1)]
    [InlineData((short)100, (short)100)]
    [InlineData((long)999999, (long)999999)]
    [InlineData((float)1.5f, (float)1.5f)]
    [InlineData((double)2.5, (double)2.5)]
    [InlineData('A', 'A')]
    public void Compare_EqualPrimitiveTypes_ReturnsNoDifferences(object a, object b)
    {
        var result = ObjectDiffer.Create().Compare(a, b);
        result.IsEqual.Should().BeTrue();
    }
}
