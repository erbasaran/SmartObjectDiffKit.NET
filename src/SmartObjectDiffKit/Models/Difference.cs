namespace SmartObjectDiffKit;

/// <summary>
/// Represents a single difference detected between two objects.
/// </summary>
public sealed class Difference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Difference"/> class.
    /// </summary>
    public Difference(
        string propertyPath,
        object? oldValue,
        object? newValue,
        Type? oldType,
        Type? newType,
        DifferenceType differenceType,
        DifferenceSeverity severity,
        int depth)
    {
        PropertyPath = propertyPath ?? throw new ArgumentNullException(nameof(propertyPath));
        OldValue = oldValue;
        NewValue = newValue;
        OldType = oldType;
        NewType = newType;
        DifferenceType = differenceType;
        Severity = severity;
        Depth = depth;
    }

    /// <summary>
    /// Gets the path of the property where the difference was detected.
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Gets the type of the old value.
    /// </summary>
    public Type? OldType { get; }

    /// <summary>
    /// Gets the type of the new value.
    /// </summary>
    public Type? NewType { get; }

    /// <summary>
    /// Gets the type of difference detected.
    /// </summary>
    public DifferenceType DifferenceType { get; }

    /// <summary>
    /// Gets the severity level of the difference.
    /// </summary>
    public DifferenceSeverity Severity { get; }

    /// <summary>
    /// Gets the depth in the object graph where the difference was found.
    /// </summary>
    public int Depth { get; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"[{DifferenceType}] {PropertyPath}: '{OldValue}' -> '{NewValue}' (Severity: {Severity}, Depth: {Depth})";
}
