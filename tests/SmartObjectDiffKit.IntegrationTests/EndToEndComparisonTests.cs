using FluentAssertions;
using SmartObjectDiffKit.Configuration;

namespace SmartObjectDiffKit.IntegrationTests;

public class EndToEndComparisonTests
{
    [Fact]
    public void FullWorkflow_CompareExportFilter_WorksEndToEnd()
    {
        var oldObj = new
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Address = new { Street = "123 Main St", City = "Springfield", Zip = "12345" },
            Orders = new[]
            {
                new { Product = "Widget", Quantity = 5, Price = 9.99m },
                new { Product = "Gadget", Quantity = 2, Price = 19.99m }
            }
        };

        var newObj = new
        {
            Id = 1,
            Name = "John Smith",
            Email = "john.smith@example.com",
            Address = new { Street = "456 Oak Ave", City = "Springfield", Zip = "12345" },
            Orders = new[]
            {
                new { Product = "Widget", Quantity = 10, Price = 9.99m },
                new { Product = "Gadget", Quantity = 2, Price = 24.99m },
                new { Product = "Doohickey", Quantity = 1, Price = 4.99m }
            }
        };

        var result = ObjectDiffer.Create()
            .MaxDepth(10)
            .Compare(oldObj, newObj);

        result.IsEqual.Should().BeFalse();
        result.DifferenceCount.Should().BeGreaterThan(0);
        result.ElapsedTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);

        var json = result.ToJson();
        json.Should().Contain("\"differenceCount\"");

        var xml = result.ToXml();
        xml.Should().Contain("<DiffResult>");

        var md = result.ToMarkdown();
        md.Should().Contain("# Diff Report");

        var html = result.ToHtml();
        html.Should().Contain("<!DOCTYPE html>");

        var csv = result.ToCsv();
        csv.Should().Contain("PropertyPath");

        var text = result.ToPlainText();
        text.Should().Contain("=== Diff Report ===");

        var console = result.ToConsole();
        console.Should().Contain("[DIFF]");
    }

    [Fact]
    public void ComplexObjectGraph_WithCircularReferences_CompletesSuccessfully()
    {
        var nodeA1 = new TreeNode("A");
        var nodeB1 = new TreeNode("B");
        var nodeC1 = new TreeNode("C");
        nodeA1.Children.Add(nodeB1);
        nodeB1.Children.Add(nodeC1);
        nodeC1.Children.Add(nodeA1);

        var nodeA2 = new TreeNode("A");
        var nodeB2 = new TreeNode("B");
        var nodeC2 = new TreeNode("C");
        nodeA2.Children.Add(nodeB2);
        nodeB2.Children.Add(nodeC2);
        nodeC2.Children.Add(nodeA2);

        var result = ObjectDiffer.Create()
            .MaxDepth(20)
            .Compare(nodeA1, nodeA2);

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void LargeCollection_Comparison_CompletesInReasonableTime()
    {
        var oldList = Enumerable.Range(0, 1000).Select(i => new { Id = i, Value = $"Item{i}" }).ToList();
        var newList = Enumerable.Range(0, 1000).Select(i => new { Id = i, Value = i % 2 == 0 ? $"Item{i}" : $"Changed{i}" }).ToList();

        var result = ObjectDiffer.Create().Compare(oldList, newList);

        result.IsEqual.Should().BeFalse();
        result.ElapsedTime.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IgnoreConfiguration_FluentApi_WorksCorrectly()
    {
        var oldObj = new Employee
        {
            Id = 1,
            Name = "John",
            Department = "Engineering",
            Salary = 100000,
            LastReview = DateTime.Now,
            Tags = new List<string> { "senior", "lead" }
        };

        var newObj = new Employee
        {
            Id = 1,
            Name = "John",
            Department = "Engineering",
            Salary = 120000,
            LastReview = DateTime.Now.AddDays(30),
            Tags = new List<string> { "senior", "lead" }
        };

        var result = ObjectDiffer.Create()
            .IgnoreProperty(nameof(Employee.LastReview))
            .IgnoreProperty(nameof(Employee.Salary))
            .Compare(oldObj, newObj);

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void StringNormalization_CombinedOptions_WorkCorrectly()
    {
        var result = ObjectDiffer.Create()
            .TrimStrings()
            .CaseInsensitive()
            .NormalizeWhitespace()
            .NormalizeLineEndings()
            .Compare("  Hello   World  \r\n", "hello world\n");

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public void DictionaryComparison_WithComplexValues_WorksCorrectly()
    {
        var oldDict = new Dictionary<string, List<int>>
        {
            { "a", new List<int> { 1, 2, 3 } },
            { "b", new List<int> { 4, 5 } }
        };

        var newDict = new Dictionary<string, List<int>>
        {
            { "a", new List<int> { 1, 2, 4 } },
            { "b", new List<int> { 4, 5 } },
            { "c", new List<int> { 6 } }
        };

        var result = ObjectDiffer.Create().Compare(oldDict, newDict);
        result.IsEqual.Should().BeFalse();
        result.Differences.Should().Contain(d => d.PropertyPath.Contains("\"c\""));
    }

    [Fact]
    public void CustomComparer_Integration_WorksCorrectly()
    {
        var result = ObjectDiffer.Create()
            .UseComparer<DateTime>(new DateOnlyComparer())
            .Compare(
                new { Date = new DateTime(2024, 1, 15, 10, 30, 0) },
                new { Date = new DateTime(2024, 1, 15, 14, 45, 0) });

        result.IsEqual.Should().BeTrue();
    }

    [Fact]
    public async Task ThreadSafety_ConcurrentComparisons_ProduceCorrectResults()
    {
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await Task.Yield();
            var a = new { Id = i, Name = $"Item{i}" };
            var b = new { Id = i, Name = $"Item{i}" };
            return ObjectDiffer.Create().Compare(a, b);
        }).ToArray();

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            result.IsEqual.Should().BeTrue();
        }
    }

    private class TreeNode
    {
        public TreeNode(string name) { Name = name; }
        public string Name { get; set; }
        public List<TreeNode> Children { get; } = new();
    }

    private class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime LastReview { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    private class DateOnlyComparer : ICustomComparer
    {
        public bool AreEqual(object? x, object? y)
        {
            if (x is DateTime dx && y is DateTime dy)
                return dx.Date == dy.Date;
            return Equals(x, y);
        }
    }
}
