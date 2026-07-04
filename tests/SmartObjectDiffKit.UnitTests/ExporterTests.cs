using FluentAssertions;
using SmartObjectDiffKit.Exporters;
using SmartObjectDiffKit.UnitTests.TestModels;

namespace SmartObjectDiffKit.UnitTests;

public class ExporterTests
{
    private readonly DiffResult _testResult;

    public ExporterTests()
    {
        var a = new SimpleObject { Id = 1, Name = "Old", Price = 9.99m };
        var b = new SimpleObject { Id = 1, Name = "New", Price = 19.99m };
        _testResult = ObjectDiffer.Create().Compare(a, b);
    }

    [Fact]
    public void JsonExporter_ProducesValidJson()
    {
        var json = _testResult.ToJson();
        json.Should().Contain("\"differenceCount\"");
        json.Should().Contain("\"differences\"");
        json.Should().Contain("Name");
    }

    [Fact]
    public void XmlExporter_ProducesValidXml()
    {
        var xml = _testResult.ToXml();
        xml.Should().Contain("<DiffResult>");
        xml.Should().Contain("<Differences>");
        xml.Should().Contain("<Difference>");
    }

    [Fact]
    public void MarkdownExporter_ProducesMarkdown()
    {
        var md = _testResult.ToMarkdown();
        md.Should().Contain("# Diff Report");
        md.Should().Contain("## Differences");
        md.Should().Contain("| Property Path |");
    }

    [Fact]
    public void HtmlExporter_ProducesHtml()
    {
        var html = _testResult.ToHtml();
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("<table>");
        html.Should().Contain("Diff Report");
    }

    [Fact]
    public void CsvExporter_ProducesCsv()
    {
        var csv = _testResult.ToCsv();
        csv.Should().Contain("PropertyPath,DifferenceType");
        csv.Should().Contain("Name");
    }

    [Fact]
    public void PlainTextExporter_ProducesText()
    {
        var text = _testResult.ToPlainText();
        text.Should().Contain("=== Diff Report ===");
        text.Should().Contain("--- Differences ---");
    }

    [Fact]
    public void ConsoleExporter_ProducesConsoleOutput()
    {
        var output = _testResult.ToConsole();
        output.Should().Contain("[DIFF]");
    }

    [Fact]
    public void EqualResult_JsonExporter_ShowsEqual()
    {
        var a = new SimpleObject { Id = 1, Name = "Test" };
        var b = new SimpleObject { Id = 1, Name = "Test" };
        var result = ObjectDiffer.Create().Compare(a, b);

        var json = result.ToJson();
        json.Should().Contain("\"isEqual\": true");
    }

    [Fact]
    public void Export_WithCustomExporter_Works()
    {
        var exporter = new PlainTextDiffExporter();
        var output = _testResult.Export(exporter);
        output.Should().Contain("=== Diff Report ===");
    }
}
