using System.Linq.Expressions;

namespace SmartObjectDiffKit.Configuration;

/// <summary>
/// Fluent builder for configuring object comparison options.
/// </summary>
public sealed class DiffBuilder
{
    private readonly DiffOptions _options = new();

    /// <summary>
    /// Ignores a property by name during comparison.
    /// </summary>
    /// <param name="propertyName">The name of the property to ignore.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreProperty(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

        _options.IgnoredPropertyNames.Add(propertyName);
        return this;
    }

    /// <summary>
    /// Ignores a property by expression during comparison.
    /// </summary>
    /// <typeparam name="T">The type containing the property.</typeparam>
    /// <param name="expression">An expression selecting the property to ignore.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreProperty<T>(Expression<Func<T, object?>> expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var memberName = GetMemberName(expression);
        _options.IgnoredPropertyNames.Add(memberName);
        return this;
    }

    /// <summary>
    /// Ignores properties matching a predicate.
    /// </summary>
    /// <param name="predicate">A predicate that receives property name and declaring type.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreProperty(Func<string, Type, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        _options.IgnoredPropertyPredicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Ignores collection order during comparison.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreCollectionOrder()
    {
        _options.IgnoreCollectionOrder = true;
        return this;
    }

    /// <summary>
    /// Trims strings before comparison.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder TrimStrings()
    {
        _options.TrimStrings = true;
        return this;
    }

    /// <summary>
    /// Uses case-insensitive string comparison.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder CaseInsensitive()
    {
        _options.CaseInsensitiveStrings = true;
        _options.StringComparison = StringComparison.OrdinalIgnoreCase;
        return this;
    }

    /// <summary>
    /// Normalizes whitespace in strings before comparison.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder NormalizeWhitespace()
    {
        _options.NormalizeWhitespace = true;
        return this;
    }

    /// <summary>
    /// Normalizes line endings in strings before comparison.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder NormalizeLineEndings()
    {
        _options.NormalizeLineEndings = true;
        return this;
    }

    /// <summary>
    /// Ignores properties with null values.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreNullValues()
    {
        _options.IgnoreNullValues = true;
        return this;
    }

    /// <summary>
    /// Ignores properties with default values.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreDefaultValues()
    {
        _options.IgnoreDefaultValues = true;
        return this;
    }

    /// <summary>
    /// Ignores read-only properties.
    /// </summary>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder IgnoreReadOnlyProperties()
    {
        _options.IgnoreReadOnlyProperties = true;
        return this;
    }

    /// <summary>
    /// Sets the maximum depth for object graph traversal.
    /// </summary>
    /// <param name="depth">The maximum depth.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder MaxDepth(int depth)
    {
        if (depth < 1)
            throw new ArgumentOutOfRangeException(nameof(depth), "Maximum depth must be at least 1.");

        _options.MaxDepth = depth;
        return this;
    }

    /// <summary>
    /// Uses culture-aware string comparison.
    /// </summary>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder UseCulture(System.Globalization.CultureInfo culture)
    {
        _options.CultureInfo = culture ?? throw new ArgumentNullException(nameof(culture));
        return this;
    }

    /// <summary>
    /// Registers a custom comparer for a specific type.
    /// </summary>
    /// <typeparam name="T">The type to compare.</typeparam>
    /// <param name="comparer">The custom comparer.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder UseComparer<T>(ICustomComparer comparer)
    {
        if (comparer == null)
            throw new ArgumentNullException(nameof(comparer));

        _options.CustomComparers[typeof(T)] = comparer;
        return this;
    }

    /// <summary>
    /// Sets the default severity for differences.
    /// </summary>
    /// <param name="severity">The default severity.</param>
    /// <returns>The current builder instance.</returns>
    public DiffBuilder WithDefaultSeverity(DifferenceSeverity severity)
    {
        _options.DefaultSeverity = severity;
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="ObjectDiffer"/> instance.
    /// </summary>
    /// <returns>A configured <see cref="ObjectDiffer"/>.</returns>
    public ObjectDiffer Build()
    {
        return new ObjectDiffer(_options);
    }

    /// <summary>
    /// Compares two objects using the current configuration.
    /// </summary>
    /// <param name="expected">The expected object.</param>
    /// <param name="actual">The actual object.</param>
    /// <returns>The diff result.</returns>
    public DiffResult Compare(object? expected, object? actual)
    {
        return Build().Compare(expected, actual);
    }

    private static string GetMemberName<T>(Expression<Func<T, object?>> expression)
    {
        var body = expression.Body;

        if (body is UnaryExpression unary)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException("Expression must be a member access expression.", nameof(expression));
    }
}
