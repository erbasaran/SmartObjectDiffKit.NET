using System.Text;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to HTML format.
/// </summary>
public sealed class HtmlDiffExporter : IDiffExporter
{
    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><title>Diff Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 2rem; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 1rem; }");
        sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #f5f5f5; font-weight: 600; }");
        sb.AppendLine(".equal { color: #22863a; } .different { color: #cb2431; }");
        sb.AppendLine(".summary { background: #f6f8fa; padding: 1rem; border-radius: 6px; margin-bottom: 1rem; }");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>Diff Report</h1>");
        sb.AppendLine("<div class=\"summary\">");
        sb.AppendLine($"<p><strong>Status:</strong> <span class=\"{(result.IsEqual ? "equal" : "different")}\">{(result.IsEqual ? "Equal" : "Different")}</span></p>");
        sb.AppendLine($"<p><strong>Differences:</strong> {result.DifferenceCount}</p>");
        sb.AppendLine($"<p><strong>Elapsed Time:</strong> {result.ElapsedTime.TotalMilliseconds:F2}ms</p>");
        sb.AppendLine($"<p><strong>Objects Compared:</strong> {result.ComparedObjectCount}</p>");
        sb.AppendLine($"<p><strong>Properties Compared:</strong> {result.ComparedPropertyCount}</p>");
        sb.AppendLine("</div>");

        if (result.Differences.Count > 0)
        {
            sb.AppendLine("<h2>Differences</h2>");
            sb.AppendLine("<table><thead><tr>");
            sb.AppendLine("<th>Property Path</th><th>Type</th><th>Old Value</th><th>New Value</th><th>Severity</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var diff in result.Differences)
            {
                var oldVal = System.Net.WebUtility.HtmlEncode(diff.OldValue?.ToString() ?? "(null)");
                var newVal = System.Net.WebUtility.HtmlEncode(diff.NewValue?.ToString() ?? "(null)");
                sb.AppendLine($"<tr><td><code>{System.Net.WebUtility.HtmlEncode(diff.PropertyPath)}</code></td>");
                sb.AppendLine($"<td>{diff.DifferenceType}</td><td>{oldVal}</td><td>{newVal}</td>");
                sb.AppendLine($"<td>{diff.Severity}</td></tr>");
            }

            sb.AppendLine("</tbody></table>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
