using System.Text;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to CSV format.
/// </summary>
public sealed class CsvDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();
        sb.AppendLine("PropertyPath,DifferenceType,OldValue,NewValue,OldType,NewType,Severity,Depth");

        foreach (var diff in result.Differences)
        {
            sb.AppendLine(string.Join(",",
                Escape(diff.PropertyPath),
                diff.DifferenceType.ToString(),
                Escape(diff.OldValue?.ToString()),
                Escape(diff.NewValue?.ToString()),
                Escape(diff.OldType?.Name),
                Escape(diff.NewType?.Name),
                diff.Severity.ToString(),
                diff.Depth.ToString()));
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        if (value == null) return "\"\"";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
