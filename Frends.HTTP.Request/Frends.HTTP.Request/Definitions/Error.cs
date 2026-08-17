using System;

namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Error details returned when the task fails and ThrowErrorOnFailure is false.
/// </summary>
public class Error
{
    /// <summary>
    /// Error message.
    /// </summary>
    /// <example>An error occurred while processing the request.</example>
    public string Message { get; internal set; }

    /// <summary>
    /// Additional error information, such as the original exception.
    /// </summary>
    /// <example>null</example>
    public Exception AdditionalInfo { get; internal set; }
}
