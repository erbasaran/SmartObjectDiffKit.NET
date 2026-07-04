using SmartObjectDiffKit.Exporters;

namespace SmartObjectDiffKit;

/// <summary>
/// Extension methods for <see cref="DiffResult"/>.
/// </summary>
public static class DiffResultExtensions
{
    /// <summary>
    /// Exports the diff result to JSON format.
    /// </summary>
    public static string ToJson(this DiffResult result, bool indented = true)
    {
        return new JsonDiffExporter(indented).Export(result);
    }

    /// <summary>
    /// Exports the diff result to XML format.
    /// </summary>
    public static string ToXml(this DiffResult result)
    {
        return new XmlDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result to Markdown format.
    /// </summary>
    public static string ToMarkdown(this DiffResult result)
    {
        return new MarkdownDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result to HTML format.
    /// </summary>
    public static string ToHtml(this DiffResult result)
    {
        return new HtmlDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result to CSV format.
    /// </summary>
    public static string ToCsv(this DiffResult result)
    {
        return new CsvDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result to plain text format.
    /// </summary>
    public static string ToPlainText(this DiffResult result)
    {
        return new PlainTextDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result to console-formatted text.
    /// </summary>
    public static string ToConsole(this DiffResult result)
    {
        return new ConsoleDiffExporter().Export(result);
    }

    /// <summary>
    /// Exports the diff result using the specified exporter.
    /// </summary>
    public static string Export(this DiffResult result, IDiffExporter exporter)
    {
        if (exporter == null) throw new ArgumentNullException(nameof(exporter));
        return exporter.Export(result);
    }

    /// <summary>
    /// Gets differences filtered by severity.
    /// </summary>
    public static IEnumerable<Difference> WithSeverity(this DiffResult result, DifferenceSeverity severity)
    {
        return result.Differences.Where(d => d.Severity == severity);
    }

    /// <summary>
    /// Gets differences filtered by type.
    /// </summary>
    public static IEnumerable<Difference> WithType(this DiffResult result, DifferenceType type)
    {
        return result.Differences.Where(d => d.DifferenceType == type);
    }

    /// <summary>
    /// Gets differences filtered by property path prefix.
    /// </summary>
    public static IEnumerable<Difference> WithPath(this DiffResult result, string pathPrefix)
    {
        return result.Differences.Where(d => d.PropertyPath.StartsWith(pathPrefix, StringComparison.Ordinal));
    }
}
