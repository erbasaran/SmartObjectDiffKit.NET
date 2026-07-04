namespace SmartObjectDiffKit;

/// <summary>
/// Specifies the key property used for matching items in collections.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DiffKeyAttribute : Attribute
{
}
