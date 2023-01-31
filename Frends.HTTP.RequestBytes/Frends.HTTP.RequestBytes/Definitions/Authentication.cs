namespace Frends.HTTP.RequestBytes.Definitions;

/// <summary>
/// Request authentication.
/// </summary>
public enum Authentication
{
    /// <summary>
    /// No authentication.
    /// </summary>
    None,
    /// <summary>
    /// Basic authentication.
    /// </summary>
    Basic,
    /// <summary>
    /// Windows authentication.
    /// </summary>
    WindowsAuthentication,
    /// <summary>
    /// Windows authentication with Integrated Security.
    /// </summary>
    WindowsIntegratedSecurity,
    /// <summary>
    /// OAuth authentication.
    /// </summary>
    OAuth,
    /// <summary>
    /// Client Certificate authentication.
    /// </summary>
    ClientCertificate
}
