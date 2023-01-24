namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Type of HTTP Request result
/// </summary>
/// <example>GET</example>
public enum ResultMethod
{
    /// <summary>
    /// HTTP result as a string.
    /// </summary>
    HTTP,
    /// <summary>
    /// REST result as an object.
    /// </summary>
    REST
}
