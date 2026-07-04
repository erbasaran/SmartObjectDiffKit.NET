namespace SmartObjectDiffKit;

/// <summary>
/// Specifies the severity level of a difference.
/// </summary>
public enum DifferenceSeverity
{
    /// <summary>Informational change with no impact.</summary>
    Info = 0,

    /// <summary>Low severity change.</summary>
    Low = 1,

    /// <summary>Medium severity change.</summary>
    Medium = 2,

    /// <summary>High severity change.</summary>
    High = 3,

    /// <summary>Critical change requiring immediate attention.</summary>
    Critical = 4
}
