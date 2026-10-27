using System;
using System.Collections.Generic;

namespace Frends.HTTP.RequestBytes.Definitions;

/// <summary>
/// Result class
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Body of response
    /// </summary>
    /// <example>{"id": "abcdefghijkl123456789",  "success": true,  "errors": []}</example>
    public byte[] Body { get; set; }

    /// <summary>
    /// Body size of response
    /// </summary>
    /// <example>10.0</example>
    public double BodySizeInMegaBytes => Math.Round(Body?.Length / (1024 * 1024d) ?? 0, 3);

    /// <summary>
    /// Header type of response
    /// </summary>
    /// <example>application/json</example>
    public string ContentType { get; set; }

    /// <summary>
    /// Headers of response
    /// </summary>
    /// <example>{[ "content-type": "application/json", ... ]}</example>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Statuscode of response
    /// </summary>
    /// <example>200</example>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
