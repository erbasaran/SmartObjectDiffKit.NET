using SmartObjectDiffKit.Configuration;
using SmartObjectDiffKit.Infrastructure;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SmartObjectDiffKit;

public sealed class ObjectDiffer
{
    private readonly DiffOptions _options;

    public ObjectDiffer() : this(new DiffOptions())
    {
    }

    internal ObjectDiffer(DiffOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public static DiffBuilder Create() => new DiffBuilder();

    public DiffResult Compare(object? expected, object? actual)
    {
        var sw = Stopwatch.StartNew();
        var context = new ComparisonContext(_options);
        var pathBuilder = new StringBuilder(256);

        CompareValues(expected, actual, pathBuilder, 0, context);

        sw.Stop();

        var stats = new DiffStatistics(
            context.VisitedObjects,
            context.ComparedProperties,
            context.ComparedCollections,
            context.MaxDepthReached,
            0);

        return new DiffResult(
            context.Differences,
            sw.Elapsed,
            context.VisitedObjects,
            context.ComparedProperties,
            context.MaxDepthReached,
            stats);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompareValues(object? expected, object? actual, StringBuilder path, int depth, ComparisonContext context)
    {
        if (depth > _options.MaxDepth)
            return;

        context.TrackDepth(depth);
        context.IncrementVisited();

        if (ReferenceEquals(expected, actual))
            return;

        if (expected is null && actual is null)
            return;

        if (expected is null || actual is null)
        {
            AddDifference(context, path, expected, actual, expected?.GetType(), actual?.GetType(), DifferenceType.NullChanged, _options.DefaultSeverity, depth);
            return;
        }

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        if (expectedType != actualType)
        {
            AddDifference(context, path, expected, actual, expectedType, actualType, DifferenceType.TypeChanged, DifferenceSeverity.High, depth);
            return;
        }

        if (_options.CustomComparers.TryGetValue(expectedType, out var customComparer))
        {
            if (!customComparer.AreEqual(expected, actual))
            {
                AddDifference(context, path, expected, actual, expectedType, actualType, DifferenceType.Modified, _options.DefaultSeverity, depth);
            }
            return;
        }

        if (ReflectionCache.IsSimpleType(expectedType))
        {
            CompareSimpleValues(expected, actual, expectedType, path, depth, context);
            return;
        }

        if (ReflectionCache.IsDictionaryType(expectedType))
        {
            CompareDictionaries(expected, actual, path, depth, context);
            return;
        }

        if (ReflectionCache.IsCollectionType(expectedType))
        {
            CompareCollections(expected, actual, path, depth, context);
            return;
        }

        CompareComplexObjects(expected, actual, expectedType, path, depth, context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompareSimpleValues(object expected, object actual, Type type, StringBuilder path, int depth, ComparisonContext context)
    {
        context.IncrementProperties();

        if (type == typeof(string))
        {
            var expectedStr = (string)expected;
            var actualStr = (string)actual;

            if (_options.TrimStrings || _options.NormalizeWhitespace || _options.NormalizeLineEndings)
            {
                expectedStr = _options.NormalizeString(expectedStr) ?? string.Empty;
                actualStr = _options.NormalizeString(actualStr) ?? string.Empty;
            }

            var cmp = _options.CaseInsensitiveStrings ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (!string.Equals(expectedStr, actualStr, cmp))
            {
                AddDifference(context, path, expected, actual, type, type, DifferenceType.Modified, _options.DefaultSeverity, depth);
            }
            return;
        }

        if (!expected.Equals(actual))
        {
            AddDifference(context, path, expected, actual, type, type, DifferenceType.Modified, _options.DefaultSeverity, depth);
        }
    }

    private void CompareComplexObjects(object expected, object actual, Type type, StringBuilder path, int depth, ComparisonContext context)
    {
        if (context.IsVisited(expected) || context.IsVisited(actual))
            return;

        context.MarkVisited(expected);
        context.MarkVisited(actual);

        var properties = ReflectionCache.GetProperties(type);
        var pathLength = path.Length;

        foreach (var prop in properties)
        {
            if (_options.ShouldIgnoreProperty(prop.Name, type))
                continue;

            if (_options.IgnoreReadOnlyProperties && !prop.PropertyInfo.CanWrite)
                continue;

            var getter = ReflectionCache.GetGetter(type, prop.Name);
            if (getter is null)
                continue;

            var valE = getter(expected);
            var valA = getter(actual);

            if (_options.IgnoreNullValues && valE is null && valA is null)
                continue;

            if (_options.IgnoreDefaultValues)
            {
                var defaultVal = DefaultValueCache.GetDefaultValue(prop.PropertyType);
                if (Equals(valE, defaultVal) && Equals(valA, defaultVal))
                    continue;
            }

            if (pathLength > 0)
            {
                path.Append('.');
            }
            path.Append(prop.DisplayName);

            context.IncrementProperties();
            CompareValues(valE, valA, path, depth + 1, context);

            path.Length = pathLength;
        }
    }

    private void CompareCollections(object expected, object actual, StringBuilder path, int depth, ComparisonContext context)
    {
        context.IncrementCollections();

        var expectedList = new List<object?>();
        var actualList = new List<object?>();

        foreach (var item in (IEnumerable)expected)
            expectedList.Add(item);

        foreach (var item in (IEnumerable)actual)
            actualList.Add(item);

        var expectedType = expected.GetType();
        var elementType = ReflectionCache.GetCollectionElementType(expectedType);
        PropertyMetadata? keyProperty = null;
        if (elementType != null)
        {
            keyProperty = ReflectionCache.GetKeyProperty(elementType);
        }

        if (keyProperty != null)
        {
            CompareCollectionsKeyed(expectedList, actualList, keyProperty, path, depth, context);
        }
        else if (_options.IgnoreCollectionOrder)
        {
            CompareCollectionsUnordered(expectedList, actualList, path, depth, context);
        }
        else
        {
            CompareCollectionsOrdered(expectedList, actualList, path, depth, context);
        }
    }

    private void CompareCollectionsOrdered(List<object?> expected, List<object?> actual, StringBuilder path, int depth, ComparisonContext context)
    {
        var maxCount = Math.Max(expected.Count, actual.Count);
        var pathLength = path.Length;

        for (var i = 0; i < maxCount; i++)
        {
            path.Length = pathLength;
            path.Append('[').Append(i).Append(']');

            if (i >= expected.Count)
            {
                AddDifference(context, path, null, actual[i], null, actual[i]?.GetType(), DifferenceType.Added, _options.DefaultSeverity, depth + 1);
            }
            else if (i >= actual.Count)
            {
                AddDifference(context, path, expected[i], null, expected[i]?.GetType(), null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
            }
            else
            {
                CompareValues(expected[i], actual[i], path, depth + 1, context);
            }
        }

        path.Length = pathLength;
    }

    private void CompareCollectionsUnordered(List<object?> expected, List<object?> actual, StringBuilder path, int depth, ComparisonContext context)
    {
        var matchedActual = new bool[actual.Count];
        var pathLength = path.Length;

        for (var i = 0; i < expected.Count; i++)
        {
            var found = false;
            for (var j = 0; j < actual.Count; j++)
            {
                if (matchedActual[j]) continue;

                if (AreValuesEqual(expected[i], actual[j]))
                {
                    matchedActual[j] = true;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                path.Length = pathLength;
                path.Append('[').Append(i).Append(']');
                AddDifference(context, path, expected[i], null, expected[i]?.GetType(), null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
            }
        }

        for (var j = 0; j < actual.Count; j++)
        {
            if (!matchedActual[j])
            {
                path.Length = pathLength;
                path.Append('[').Append(j).Append(']');
                AddDifference(context, path, null, actual[j], null, actual[j]?.GetType(), DifferenceType.Added, _options.DefaultSeverity, depth + 1);
            }
        }

        path.Length = pathLength;
    }

    private void CompareCollectionsKeyed(List<object?> expected, List<object?> actual, PropertyMetadata keyProperty, StringBuilder path, int depth, ComparisonContext context)
    {
        var elementType = keyProperty.PropertyInfo.DeclaringType;
        if (elementType == null)
        {
            CompareCollectionsOrdered(expected, actual, path, depth, context);
            return;
        }

        var getter = ReflectionCache.GetGetter(elementType, keyProperty.Name);
        if (getter == null)
        {
            CompareCollectionsOrdered(expected, actual, path, depth, context);
            return;
        }

        var actualByKey = new Dictionary<object, (object? Item, int Index)>();
        for (var i = 0; i < actual.Count; i++)
        {
            var item = actual[i];
            if (item != null)
            {
                var keyVal = getter(item);
                if (keyVal != null)
                {
                    actualByKey[keyVal] = (item, i);
                }
            }
        }

        var matchedActualIndices = new HashSet<int>();
        var pathLength = path.Length;

        for (var i = 0; i < expected.Count; i++)
        {
            path.Length = pathLength;
            var expectedItem = expected[i];
            if (expectedItem == null)
            {
                path.Append('[').Append(i).Append(']');
                AddDifference(context, path, null, null, null, null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
                continue;
            }

            var keyVal = getter(expectedItem);
            if (keyVal == null)
            {
                path.Append('[').Append(i).Append(']');
                AddDifference(context, path, expectedItem, null, expectedItem.GetType(), null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
                continue;
            }

            if (actualByKey.TryGetValue(keyVal, out var actualMatch))
            {
                matchedActualIndices.Add(actualMatch.Index);
                if (keyVal is string s)
                    path.Append("[\"").Append(s).Append("\"]");
                else
                    path.Append('[').Append(keyVal).Append(']');

                CompareValues(expectedItem, actualMatch.Item, path, depth + 1, context);
            }
            else
            {
                path.Append('[').Append(i).Append(']');
                AddDifference(context, path, expectedItem, null, expectedItem.GetType(), null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
            }
        }

        for (var j = 0; j < actual.Count; j++)
        {
            if (!matchedActualIndices.Contains(j))
            {
                var actualItem = actual[j];
                path.Length = pathLength;
                if (actualItem != null)
                {
                    var keyVal = getter(actualItem);
                    if (keyVal != null)
                    {
                        if (keyVal is string s)
                            path.Append("[\"").Append(s).Append("\"]");
                        else
                            path.Append('[').Append(keyVal).Append(']');
                    }
                    else
                    {
                        path.Append('[').Append(j).Append(']');
                    }
                }
                else
                {
                    path.Append('[').Append(j).Append(']');
                }
                AddDifference(context, path, null, actualItem, null, actualItem?.GetType(), DifferenceType.Added, _options.DefaultSeverity, depth + 1);
            }
        }

        path.Length = pathLength;
    }

    private void CompareDictionaries(object expected, object actual, StringBuilder path, int depth, ComparisonContext context)
    {
        context.IncrementCollections();

        var expectedDict = (IDictionary)expected;
        var actualDict = (IDictionary)actual;

        var expectedKeys = new HashSet<object>();
        foreach (var key in expectedDict.Keys)
            expectedKeys.Add(key);

        var actualKeys = new HashSet<object>();
        foreach (var key in actualDict.Keys)
            actualKeys.Add(key);

        var pathLength = path.Length;

        foreach (var key in expectedKeys)
        {
            path.Length = pathLength;
            if (key is string s)
                path.Append("[\"").Append(s).Append("\"]");
            else
                path.Append('[').Append(key).Append(']');

            if (!actualKeys.Contains(key))
            {
                AddDifference(context, path, expectedDict[key], null, expectedDict[key]?.GetType(), null, DifferenceType.Removed, _options.DefaultSeverity, depth + 1);
            }
            else
            {
                CompareValues(expectedDict[key], actualDict[key], path, depth + 1, context);
            }
        }

        foreach (var key in actualKeys)
        {
            if (!expectedKeys.Contains(key))
            {
                path.Length = pathLength;
                if (key is string s)
                    path.Append("[\"").Append(s).Append("\"]");
                else
                    path.Append('[').Append(key).Append(']');
                AddDifference(context, path, null, actualDict[key], null, actualDict[key]?.GetType(), DifferenceType.Added, _options.DefaultSeverity, depth + 1);
            }
        }

        path.Length = pathLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AreValuesEqual(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;

        if (a is string sa && b is string sb)
        {
            if (_options.TrimStrings || _options.NormalizeWhitespace || _options.NormalizeLineEndings)
            {
                sa = _options.NormalizeString(sa) ?? string.Empty;
                sb = _options.NormalizeString(sb) ?? string.Empty;
            }

            var cmp = _options.CaseInsensitiveStrings ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(sa, sb, cmp);
        }

        var type = a.GetType();
        if (ReflectionCache.IsSimpleType(type))
            return a.Equals(b);

        // Fallback for complex objects: do structural comparison
        var tempContext = new ComparisonContext(_options);
        var tempPath = new StringBuilder();
        CompareValues(a, b, tempPath, 0, tempContext);
        return tempContext.Differences.Count == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddDifference(ComparisonContext context, StringBuilder path, object? oldValue, object? newValue, Type? oldType, Type? newType, DifferenceType differenceType, DifferenceSeverity severity, int depth)
    {
        var pathString = path.Length > 0 ? path.ToString() : "(root)";
        context.AddDifference(new Difference(pathString, oldValue, newValue, oldType, newType, differenceType, severity, depth));
    }
}
