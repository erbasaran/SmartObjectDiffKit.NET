using SmartObjectDiffKit.Configuration;
using System.Runtime.CompilerServices;

namespace SmartObjectDiffKit;

internal sealed class ComparisonContext
{
    private readonly HashSet<object> _visitedReferences;
    private readonly List<Difference> _differences;
    private readonly DiffOptions _options;
    private int _maxDepthReached;
    private int _visitedObjects;
    private int _comparedProperties;
    private int _comparedCollections;

    public ComparisonContext(DiffOptions options)
    {
        _options = options;
        _visitedReferences = new HashSet<object>(new ObjectReferenceEqualityComparer());
        _differences = new List<Difference>();
    }

    public IReadOnlyList<Difference> Differences => _differences;
    public int VisitedObjects => _visitedObjects;
    public int ComparedProperties => _comparedProperties;
    public int ComparedCollections => _comparedCollections;
    public int MaxDepthReached => _maxDepthReached;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddDifference(Difference difference) => _differences.Add(difference);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrackDepth(int depth)
    {
        if (depth > _maxDepthReached)
            _maxDepthReached = depth;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementVisited() => _visitedObjects++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementProperties() => _comparedProperties++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementCollections() => _comparedCollections++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsVisited(object obj) => _visitedReferences.Contains(obj);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MarkVisited(object obj)
    {
        if (obj is not null && !obj.GetType().IsValueType)
            _visitedReferences.Add(obj);
    }
}

internal sealed class ObjectReferenceEqualityComparer : IEqualityComparer<object>
{
    public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}
