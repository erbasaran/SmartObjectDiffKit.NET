using System.Text;

namespace SmartObjectDiffKit.Configuration;

public sealed class DiffOptions
{
    internal HashSet<string> IgnoredPropertyNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    internal List<Func<string, Type, bool>> IgnoredPropertyPredicates { get; } = new();
    internal bool IgnoreCollectionOrder { get; set; }
    internal bool TrimStrings { get; set; }
    internal bool CaseInsensitiveStrings { get; set; }
    internal bool NormalizeWhitespace { get; set; }
    internal bool NormalizeLineEndings { get; set; }
    internal bool IgnoreNullValues { get; set; }
    internal bool IgnoreDefaultValues { get; set; }
    internal bool IgnoreReadOnlyProperties { get; set; }
    internal bool IgnorePrivateFields { get; set; } = true;
    internal bool IgnoreBackingFields { get; set; } = true;
    internal int MaxDepth { get; set; } = 32;
    internal StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
    internal System.Globalization.CultureInfo? CultureInfo { get; set; }
    internal Dictionary<Type, ICustomComparer> CustomComparers { get; } = new();
    internal List<Func<string, bool>> PropertyPathFilters { get; } = new();
    internal DifferenceSeverity DefaultSeverity { get; set; } = DifferenceSeverity.Medium;

    private readonly ThreadLocal<StringBuilder> _stringBuilder = new(() => new StringBuilder(256));

    internal bool ShouldIgnoreProperty(string propertyName, Type declaringType)
    {
        if (IgnoredPropertyNames.Contains(propertyName))
            return true;

        foreach (var predicate in IgnoredPropertyPredicates)
        {
            if (predicate(propertyName, declaringType))
                return true;
        }

        return false;
    }

    internal StringComparer GetStringComparer()
    {
        return CaseInsensitiveStrings
            ? StringComparer.Create(CultureInfo ?? System.Globalization.CultureInfo.CurrentCulture, true)
            : StringComparer.Create(CultureInfo ?? System.Globalization.CultureInfo.CurrentCulture, false);
    }

    internal string? NormalizeString(string? value)
    {
        if (value is null) return null;
        if (value.Length == 0) return value;

        var needsTrim = TrimStrings;
        var needsWhitespace = NormalizeWhitespace;
        var needsLineEndings = NormalizeLineEndings;

        if (!needsTrim && !needsWhitespace && !needsLineEndings)
            return value;

        if (needsLineEndings)
            value = value.Replace("\r\n", "\n").Replace('\r', '\n');

        if (needsTrim)
            value = value.Trim();

        if (needsWhitespace)
        {
            var sb = _stringBuilder.Value!;
            sb.Clear();
            sb.EnsureCapacity(value.Length);

            var lastWasSpace = false;
            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace)
                    {
                        sb.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    sb.Append(c);
                    lastWasSpace = false;
                }
            }
            value = sb.ToString();
        }

        return value;
    }
}

public interface ICustomComparer
{
    bool AreEqual(object? x, object? y);
}
