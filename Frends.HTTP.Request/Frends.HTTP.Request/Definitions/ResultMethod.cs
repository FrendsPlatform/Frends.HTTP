namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Type of HTTP Request result
/// </summary>
/// <example>GET</example>
public enum ReturnFormat
{
    /// <summary>
    /// HTTP result as a string.
    /// </summary>
    String,
    /// <summary>
    /// REST result as an JToken.
    /// </summary>
    JToken
}
