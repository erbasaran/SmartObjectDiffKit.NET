namespace SmartObjectDiffKit.Comparers;

/// <summary>
/// Default value comparer using Equals method.
/// </summary>
/// <typeparam name="T">The type of values to compare.</typeparam>
public sealed class DefaultValueComparer<T> : IValueComparer<T>
{
    /// <inheritdoc/>
    public bool AreEqual(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }
}
