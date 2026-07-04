using System.Text;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to a formatted console output with colors.
/// </summary>
public sealed class ConsoleDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();

        if (result.IsEqual)
        {
            sb.AppendLine("[OK] Objects are equal.");
            return sb.ToString();
        }

        sb.AppendLine($"[DIFF] Found {result.DifferenceCount} difference(s) in {result.ElapsedTime.TotalMilliseconds:F2}ms");
        sb.AppendLine(new string('-', 60));

        foreach (var diff in result.Differences)
        {
            var icon = diff.DifferenceType switch
            {
                DifferenceType.Added => "+",
                DifferenceType.Removed => "-",
                DifferenceType.Modified => "~",
                DifferenceType.TypeChanged => "!",
                DifferenceType.NullChanged => "0",
                DifferenceType.Moved => ">",
                _ => "?"
            };

            sb.AppendLine($"  [{icon}] {diff.PropertyPath}");
            sb.AppendLine($"      Old: {diff.OldValue ?? "(null)"}");
            sb.AppendLine($"      New: {diff.NewValue ?? "(null)"}");
        }

        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"  Objects: {result.ComparedObjectCount} | Properties: {result.ComparedPropertyCount} | Depth: {result.MaximumDepth}");

        return sb.ToString();
    }
}
