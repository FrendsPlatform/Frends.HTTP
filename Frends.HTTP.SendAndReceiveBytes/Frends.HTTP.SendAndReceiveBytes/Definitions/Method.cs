namespace Frends.HTTP.SendAndReceiveBytes.Definitions;

/// <summary>
/// Type of HTTP Request
/// </summary>
/// <example>GET</example>
public enum Method
{
    /// <summary>
    /// POST Request.
    /// </summary>
    POST,
    /// <summary>
    /// PUT Request.
    /// </summary>
    PUT,
    /// <summary>
    /// PATCH Request.
    /// </summary>
    PATCH,
    /// <summary>
    /// DELETE Request.
    /// </summary>
    DELETE,
    /// <summary>
    /// HEAD Request.
    /// </summary>
    HEAD,
    /// <summary>
    /// OPTIONS Request.
    /// </summary>
    OPTIONS,
    /// <summary>
    /// CONNECT Request.
    /// </summary>
    CONNECT
}
