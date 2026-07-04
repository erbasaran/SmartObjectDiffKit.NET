using System.Text;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to Markdown format.
/// </summary>
public sealed class MarkdownDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();

        sb.AppendLine("# Diff Report");
        sb.AppendLine();
        sb.AppendLine($"**Status:** {(result.IsEqual ? "Equal" : "Different")}");
        sb.AppendLine($"**Differences:** {result.DifferenceCount}");
        sb.AppendLine($"**Elapsed Time:** {result.ElapsedTime.TotalMilliseconds:F2}ms");
        sb.AppendLine($"**Objects Compared:** {result.ComparedObjectCount}");
        sb.AppendLine($"**Properties Compared:** {result.ComparedPropertyCount}");
        sb.AppendLine($"**Maximum Depth:** {result.MaximumDepth}");
        sb.AppendLine();

        if (result.Differences.Count > 0)
        {
            sb.AppendLine("## Differences");
            sb.AppendLine();
            sb.AppendLine("| Property Path | Type | Old Value | New Value | Severity |");
            sb.AppendLine("|---|---|---|---|---|");

            foreach (var diff in result.Differences)
            {
                var oldVal = diff.OldValue?.ToString() ?? "(null)";
                var newVal = diff.NewValue?.ToString() ?? "(null)";
                sb.AppendLine($"| `{diff.PropertyPath}` | {diff.DifferenceType} | {EscapeMarkdown(oldVal)} | {EscapeMarkdown(newVal)} | {diff.Severity} |");
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Statistics");
        sb.AppendLine();
        sb.AppendLine($"- Visited Objects: {result.Statistics.VisitedObjects}");
        sb.AppendLine($"- Compared Properties: {result.Statistics.ComparedProperties}");
        sb.AppendLine($"- Compared Collections: {result.Statistics.ComparedCollections}");
        sb.AppendLine($"- Maximum Depth Reached: {result.Statistics.MaximumDepthReached}");

        return sb.ToString();
    }

    private static string EscapeMarkdown(string value)
    {
        return value.Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");
    }
}
