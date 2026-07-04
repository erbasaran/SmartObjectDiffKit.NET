using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SmartObjectDiffKit.Infrastructure;

internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> s_properties = new();
    private static readonly ConcurrentDictionary<Type, FieldMetadata[]> s_fields = new();
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object?>?> s_getters = new();
    private static readonly ConcurrentDictionary<Type, bool> s_isSimpleType = new();
    private static readonly ConcurrentDictionary<Type, bool> s_isCollectionType = new();
    private static readonly ConcurrentDictionary<Type, bool> s_isDictionaryType = new();
    private static readonly ConcurrentDictionary<Type, Type?> s_collectionElementType = new();
    private static readonly ConcurrentDictionary<Type, (Type? Key, Type? Value)> s_dictionaryTypes = new();
    private static readonly ConcurrentDictionary<Type, PropertyMetadata?> s_keyProperties = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyMetadata[] GetProperties(Type type)
    {
        return s_properties.GetOrAdd(type, static t => BuildPropertyMetadata(t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyMetadata? GetKeyProperty(Type type)
    {
        return s_keyProperties.GetOrAdd(type, static t =>
        {
            var properties = GetProperties(t);
            for (var i = 0; i < properties.Length; i++)
            {
                if (properties[i].IsKey)
                    return properties[i];
            }
            return null;
        });
    }

    private static PropertyMetadata[] BuildPropertyMetadata(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new List<PropertyMetadata>(props.Length);

        foreach (var prop in props)
        {
            if (prop.GetCustomAttribute<DiffIgnoreAttribute>(true) != null)
                continue;

            if (prop.GetIndexParameters().Length > 0)
                continue;

            var orderAttr = prop.GetCustomAttribute<DiffOrderAttribute>(true);
            var displayAttr = prop.GetCustomAttribute<DiffDisplayNameAttribute>(true);
            var keyAttr = prop.GetCustomAttribute<DiffKeyAttribute>(true);

            result.Add(new PropertyMetadata(
                prop.Name,
                prop.PropertyType,
                prop,
                orderAttr?.Order ?? int.MaxValue,
                displayAttr?.DisplayName ?? prop.Name,
                keyAttr != null));
        }

        result.Sort((a, b) =>
        {
            var cmp = a.Order.CompareTo(b.Order);
            return cmp != 0 ? cmp : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });

        return result.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<object, object?>? GetGetter(Type type, string propertyName)
    {
        return s_getters.GetOrAdd((type, propertyName), static key => BuildGetter(key.Item1, key.Item2));
    }

    private static Func<object, object?>? BuildGetter(Type type, string propertyName)
    {
        var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || !prop.CanRead)
            return null;

        try
        {
            var param = Expression.Parameter(typeof(object), "instance");
            var cast = Expression.Convert(param, type);
            var access = Expression.Property(cast, prop);
            var box = Expression.Convert(access, typeof(object));
            return Expression.Lambda<Func<object, object?>>(box, param).Compile();
        }
        catch
        {
            return null;
        }
    }

    public static FieldMetadata[] GetFields(Type type)
    {
        return s_fields.GetOrAdd(type, static t =>
        {
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<FieldMetadata>(fields.Length);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<DiffIgnoreAttribute>(true) != null)
                    continue;

                result.Add(new FieldMetadata(field.Name, field.FieldType, field));
            }

            return result.ToArray();
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSimpleType(Type type)
    {
        return s_isSimpleType.GetOrAdd(type, static t =>
        {
            var underlying = Nullable.GetUnderlyingType(t) ?? t;
            return underlying.IsPrimitive
                || underlying.IsEnum
                || underlying == typeof(string)
                || underlying == typeof(decimal)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(TimeSpan)
                || underlying == typeof(Guid);
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCollectionType(Type type)
    {
        return s_isCollectionType.GetOrAdd(type, static t =>
        {
            if (t == typeof(string)) return false;
            if (t.IsArray) return true;
            if (t.IsGenericType)
            {
                var gd = t.GetGenericTypeDefinition();
                if (gd == typeof(List<>)
                    || gd == typeof(IList<>)
                    || gd == typeof(ICollection<>)
                    || gd == typeof(IEnumerable<>)
                    || gd == typeof(HashSet<>)
                    || gd == typeof(SortedSet<>)
                    || gd == typeof(Queue<>)
                    || gd == typeof(Stack<>)
                    || gd == typeof(LinkedList<>))
                    return true;
            }
            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return true;
            }
            return false;
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDictionaryType(Type type)
    {
        return s_isDictionaryType.GetOrAdd(type, static t =>
        {
            if (t.IsGenericType)
            {
                var gd = t.GetGenericTypeDefinition();
                if (gd == typeof(Dictionary<,>)
                    || gd == typeof(IDictionary<,>)
                    || gd == typeof(IReadOnlyDictionary<,>)
                    || gd == typeof(ConcurrentDictionary<,>))
                    return true;
            }
            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    return true;
            }
            return false;
        });
    }

    public static Type? GetCollectionElementType(Type type)
    {
        return s_collectionElementType.GetOrAdd(type, static t =>
        {
            if (t.IsArray)
                return t.GetElementType();

            if (t.IsGenericType)
            {
                var args = t.GetGenericArguments();
                var gd = t.GetGenericTypeDefinition();
                if (gd == typeof(Dictionary<,>) || gd == typeof(IDictionary<,>)
                    || gd == typeof(IReadOnlyDictionary<,>) || gd == typeof(ConcurrentDictionary<,>))
                    return null;

                if (args.Length == 1)
                    return args[0];
            }

            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return iface.GetGenericArguments()[0];
            }

            return null;
        });
    }

    public static (Type? Key, Type? Value) GetDictionaryTypes(Type type)
    {
        return s_dictionaryTypes.GetOrAdd(type, static t =>
        {
            if (t.IsGenericType)
            {
                var args = t.GetGenericArguments();
                var gd = t.GetGenericTypeDefinition();
                if (gd == typeof(Dictionary<,>) || gd == typeof(IDictionary<,>)
                    || gd == typeof(IReadOnlyDictionary<,>) || gd == typeof(ConcurrentDictionary<,>))
                {
                    if (args.Length == 2)
                        return (args[0], args[1]);
                }
            }

            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var args = iface.GetGenericArguments();
                    if (args.Length == 2)
                        return (args[0], args[1]);
                }
            }

            return (null, null);
        });
    }

    public static void Clear()
    {
        s_properties.Clear();
        s_fields.Clear();
        s_getters.Clear();
        s_isSimpleType.Clear();
        s_isCollectionType.Clear();
        s_isDictionaryType.Clear();
        s_collectionElementType.Clear();
        s_dictionaryTypes.Clear();
        s_keyProperties.Clear();
    }
}

internal sealed class PropertyMetadata
{
    public PropertyMetadata(string name, Type propertyType, PropertyInfo propertyInfo, int order, string displayName, bool isKey)
    {
        Name = name;
        PropertyType = propertyType;
        PropertyInfo = propertyInfo;
        Order = order;
        DisplayName = displayName;
        IsKey = isKey;
    }

    public string Name { get; }
    public Type PropertyType { get; }
    public PropertyInfo PropertyInfo { get; }
    public int Order { get; }
    public string DisplayName { get; }
    public bool IsKey { get; }
}

internal sealed class FieldMetadata
{
    public FieldMetadata(string name, Type fieldType, FieldInfo fieldInfo)
    {
        Name = name;
        FieldType = fieldType;
        FieldInfo = fieldInfo;
    }

    public string Name { get; }
    public Type FieldType { get; }
    public FieldInfo FieldInfo { get; }
}
