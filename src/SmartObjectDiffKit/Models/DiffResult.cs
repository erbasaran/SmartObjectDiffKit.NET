using System.Collections.ObjectModel;

namespace SmartObjectDiffKit;

/// <summary>
/// Represents the result of a comparison operation between two objects.
/// </summary>
public sealed class DiffResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffResult"/> class.
    /// </summary>
    public DiffResult(
        IReadOnlyList<Difference> differences,
        TimeSpan elapsedTime,
        int comparedObjectCount,
        int comparedPropertyCount,
        int maximumDepth,
        DiffStatistics statistics)
    {
        if (differences == null) throw new ArgumentNullException(nameof(differences));

        var list = differences as IList<Difference> ?? new List<Difference>(differences);
        Differences = new ReadOnlyCollection<Difference>(list);
        ElapsedTime = elapsedTime;
        ComparedObjectCount = comparedObjectCount;
        ComparedPropertyCount = comparedPropertyCount;
        MaximumDepth = maximumDepth;
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    }

    /// <summary>
    /// Gets a value indicating whether the compared objects are equal.
    /// </summary>
    public bool IsEqual => Differences.Count == 0;

    /// <summary>
    /// Gets the total number of differences found.
    /// </summary>
    public int DifferenceCount => Differences.Count;

    /// <summary>
    /// Gets the elapsed time of the comparison operation.
    /// </summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Gets the number of objects compared.
    /// </summary>
    public int ComparedObjectCount { get; }

    /// <summary>
    /// Gets the number of properties compared.
    /// </summary>
    public int ComparedPropertyCount { get; }

    /// <summary>
    /// Gets the maximum depth reached during comparison.
    /// </summary>
    public int MaximumDepth { get; }

    /// <summary>
    /// Gets the list of differences found.
    /// </summary>
    public IReadOnlyList<Difference> Differences { get; }

    /// <summary>
    /// Gets the comparison statistics.
    /// </summary>
    public DiffStatistics Statistics { get; }

    /// <summary>
    /// Returns a string representation of the diff result.
    /// </summary>
    public override string ToString() =>
        IsEqual
            ? "Objects are equal."
            : $"Found {DifferenceCount} difference(s) in {ElapsedTime.TotalMilliseconds:F2}ms.";
}
