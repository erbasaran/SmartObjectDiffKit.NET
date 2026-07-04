namespace SmartObjectDiffKit;

/// <summary>
/// Specifies the display order of a property in diff reports.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DiffOrderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffOrderAttribute"/> class.
    /// </summary>
    /// <param name="order">The display order.</param>
    public DiffOrderAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int Order { get; }
}
