namespace SmartObjectDiffKit;

/// <summary>
/// Indicates that the decorated property should be ignored during comparison.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class DiffIgnoreAttribute : Attribute
{
}
