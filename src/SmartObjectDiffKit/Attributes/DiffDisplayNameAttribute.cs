namespace SmartObjectDiffKit;

/// <summary>
/// Specifies a custom display name for a property in diff reports.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DiffDisplayNameAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffDisplayNameAttribute"/> class.
    /// </summary>
    /// <param name="displayName">The display name.</param>
    public DiffDisplayNameAttribute(string displayName)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName { get; }
}
