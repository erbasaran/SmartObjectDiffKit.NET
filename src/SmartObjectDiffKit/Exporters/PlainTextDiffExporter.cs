using System.Text;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to plain text format.
/// </summary>
public sealed class PlainTextDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();

        sb.AppendLine("=== Diff Report ===");
        sb.AppendLine($"Status: {(result.IsEqual ? "Equal" : "Different")}");
        sb.AppendLine($"Differences: {result.DifferenceCount}");
        sb.AppendLine($"Elapsed Time: {result.ElapsedTime.TotalMilliseconds:F2}ms");
        sb.AppendLine($"Objects Compared: {result.ComparedObjectCount}");
        sb.AppendLine($"Properties Compared: {result.ComparedPropertyCount}");
        sb.AppendLine($"Maximum Depth: {result.MaximumDepth}");
        sb.AppendLine();

        if (result.Differences.Count > 0)
        {
            sb.AppendLine("--- Differences ---");
            foreach (var diff in result.Differences)
            {
                sb.AppendLine($"  [{diff.DifferenceType}] {diff.PropertyPath}");
                sb.AppendLine($"    Old: {diff.OldValue ?? "(null)"}");
                sb.AppendLine($"    New: {diff.NewValue ?? "(null)"}");
                sb.AppendLine($"    Severity: {diff.Severity} | Depth: {diff.Depth}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("--- Statistics ---");
        sb.AppendLine($"  Visited Objects: {result.Statistics.VisitedObjects}");
        sb.AppendLine($"  Compared Properties: {result.Statistics.ComparedProperties}");
        sb.AppendLine($"  Compared Collections: {result.Statistics.ComparedCollections}");
        sb.AppendLine($"  Maximum Depth Reached: {result.Statistics.MaximumDepthReached}");

        return sb.ToString();
    }
}
