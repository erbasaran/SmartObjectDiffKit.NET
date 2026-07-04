namespace SmartObjectDiffKit;

/// <summary>
/// Specifies the type of difference detected between two values.
/// </summary>
public enum DifferenceType
{
    /// <summary>A value was modified.</summary>
    Modified = 0,

    /// <summary>A property or item was added.</summary>
    Added = 1,

    /// <summary>A property or item was removed.</summary>
    Removed = 2,

    /// <summary>An item was moved to a different position.</summary>
    Moved = 3,

    /// <summary>The type of the value changed.</summary>
    TypeChanged = 4,

    /// <summary>A collection was changed.</summary>
    CollectionChanged = 5,

    /// <summary>A null reference changed to a value or vice versa.</summary>
    NullChanged = 6,

    /// <summary>An object reference changed.</summary>
    ReferenceChanged = 7
}
