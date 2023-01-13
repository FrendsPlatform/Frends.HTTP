using System.ComponentModel;

namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Options class
/// </summary>
public class Options
{
    /// <summary>
    /// Timeout value in seconds
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Transaction isolation level to use.
    /// </summary>
    /// <example>ReadCommited</example>
    [DefaultValue(TransactionIsolationLevel.RepeatableRead)]
    public TransactionIsolationLevel TransactionIsolationLevel { get; set; }
}
