using System.Text.Json;

namespace SmartObjectDiffKit.Exporters;

/// <summary>
/// Exports diff results to JSON format.
/// </summary>
public sealed class JsonDiffExporter : IDiffExporter
{
    private readonly bool _indented;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDiffExporter"/> class.
    /// </summary>
    /// <param name="indented">Whether to indent the output.</param>
    public JsonDiffExporter(bool indented = true)
    {
        _indented = indented;
    }

    /// <inheritdoc/>
    public string Export(DiffResult result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var options = new JsonSerializerOptions
        {
            WriteIndented = _indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var data = new
        {
            isEqual = result.IsEqual,
            differenceCount = result.DifferenceCount,
            elapsedTime = result.ElapsedTime.TotalMilliseconds,
            comparedObjectCount = result.ComparedObjectCount,
            comparedPropertyCount = result.ComparedPropertyCount,
            maximumDepth = result.MaximumDepth,
            differences = result.Differences.Select(d => new
            {
                propertyPath = d.PropertyPath,
                oldValue = d.OldValue?.ToString(),
                newValue = d.NewValue?.ToString(),
                oldType = d.OldType?.Name,
                newType = d.NewType?.Name,
                differenceType = d.DifferenceType.ToString(),
                severity = d.Severity.ToString(),
                depth = d.Depth
            }),
            statistics = new
            {
                visitedObjects = result.Statistics.VisitedObjects,
                comparedProperties = result.Statistics.ComparedProperties,
                comparedCollections = result.Statistics.ComparedCollections,
                maximumDepthReached = result.Statistics.MaximumDepthReached
            }
        };

        return JsonSerializer.Serialize(data, options);
    }
}
