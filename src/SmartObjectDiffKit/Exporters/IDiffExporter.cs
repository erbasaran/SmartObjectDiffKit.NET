namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Interface for exporting diff results to various formats.
/// </summary>
public interface IDiffExporter
{
    /// <summary>
    /// Exports the diff result to a string.
    /// </summary>
    /// <param name="result">The diff result to export.</param>
    /// <returns>The formatted string.</returns>
    string Export(DiffResult result);
}
