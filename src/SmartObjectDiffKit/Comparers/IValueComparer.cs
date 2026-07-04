namespace SmartObjectDiffKit.Comparers;

/// <summary>
/// Interface for comparing values of a specific type.
/// </summary>
/// <typeparam name="T">The type of values to compare.</typeparam>
public interface IValueComparer<in T>
{
    /// <summary>
    /// Compares two values and returns true if they are equal.
    /// </summary>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <returns>True if the values are equal; otherwise, false.</returns>
    bool AreEqual(T? x, T? y);
}
