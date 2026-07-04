using System.Collections.Concurrent;

namespace SmartObjectDiffKit.Infrastructure;

internal static class DefaultValueCache
{
    private static readonly ConcurrentDictionary<Type, object?> s_defaults = new();

    public static object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        return s_defaults.GetOrAdd(type, static t =>
        {
            try
            {
                return Activator.CreateInstance(t);
            }
            catch
            {
                return null;
            }
        });
    }

    public static void Clear() => s_defaults.Clear();
}
