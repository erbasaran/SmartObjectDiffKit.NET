using FluentAssertions;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class DiffResultExtensionTests
{
    private readonly DiffResult _testResult;

    public DiffResultExtensionTests()
    {
        var a = new Customer
        {
            Id = 1,
            Name = "Old",
            Address = new Address { Street = "Old St", City = "Old City", ZipCode = "00000" }
        };
        var b = new Customer
        {
            Id = 2,
            Name = "New",
            Address = new Address { Street = "New St", City = "New City", ZipCode = "00000" }
        };
        _testResult = ObjectDiffer.Create().Compare(a, b);
    }

    [Fact]
    public void WithSeverity_FiltersCorrectly()
    {
        var filtered = _testResult.WithSeverity(DifferenceSeverity.Medium);
        filtered.Should().NotBeEmpty();
    }

    [Fact]
    public void WithType_FiltersCorrectly()
    {
        var filtered = _testResult.WithType(DifferenceType.Modified);
        filtered.Should().NotBeEmpty();
    }

    [Fact]
    public void WithPath_FiltersCorrectly()
    {
        var filtered = _testResult.WithPath("Address");
        filtered.Should().NotBeEmpty();
        filtered.Should().OnlyContain(d => d.PropertyPath.StartsWith("Address"));
    }

    [Fact]
    public void ToString_EqualResult_ShowsEqual()
    {
        var a = new SimpleObject { Id = 1, Name = "Test" };
        var b = new SimpleObject { Id = 1, Name = "Test" };
        var result = ObjectDiffer.Create().Compare(a, b);

        result.ToString().Should().Be("Objects are equal.");
    }

    [Fact]
    public void ToString_DifferentResult_ShowsCount()
    {
        _testResult.ToString().Should().Contain("difference(s)");
    }

    [Fact]
    public void Statistics_ArePopulated()
    {
        _testResult.Statistics.VisitedObjects.Should().BeGreaterThan(0);
        _testResult.Statistics.ComparedProperties.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ElapsedTime_IsPositive()
    {
        _testResult.ElapsedTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }
}
