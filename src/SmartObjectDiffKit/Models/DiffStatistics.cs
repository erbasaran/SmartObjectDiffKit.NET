namespace SmartObjectDiffKit;

/// <summary>
/// Contains statistics about the comparison operation.
/// </summary>
public sealed class DiffStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffStatistics"/> class.
    /// </summary>
    public DiffStatistics(
        int visitedObjects,
        int comparedProperties,
        int comparedCollections,
        int maximumDepthReached,
        long estimatedAllocations)
    {
        VisitedObjects = visitedObjects;
        ComparedProperties = comparedProperties;
        ComparedCollections = comparedCollections;
        MaximumDepthReached = maximumDepthReached;
        EstimatedAllocations = estimatedAllocations;
    }

    /// <summary>
    /// Gets the total number of objects visited during comparison.
    /// </summary>
    public int VisitedObjects { get; }

    /// <summary>
    /// Gets the total number of properties compared.
    /// </summary>
    public int ComparedProperties { get; }

    /// <summary>
    /// Gets the total number of collections compared.
    /// </summary>
    public int ComparedCollections { get; }

    /// <summary>
    /// Gets the maximum depth reached in the object graph.
    /// </summary>
    public int MaximumDepthReached { get; }

    /// <summary>
    /// Gets the estimated memory allocations in bytes.
    /// </summary>
    public long EstimatedAllocations { get; }
}
