namespace Frends.HTTP.UploadFile.Definitions;

/// <summary>
/// Represents the authentication method to be used with the request.
/// </summary>
public enum Authentication
{
    /// <summary>
    /// No authentication is used.
    /// </summary>
    None,

    /// <summary>
    /// Basic authentication is used, where the username and password are sent in plain text.
    /// </summary>
    Basic,

    /// <summary>
    /// Windows authentication is used, where the user's Windows login credentials are used to authenticate the request.
    /// </summary>
    WindowsAuthentication,

    /// <summary>
    /// Windows integrated security is used, where the current Windows user's credentials are used to authenticate the request.
    /// </summary>
    WindowsIntegratedSecurity,

    /// <summary>
    /// OAuth token-based authentication is used, where a token is obtained from an authentication server and used to authenticate the request.
    /// </summary>
    OAuth,

    /// <summary>
    /// Client certificate-based authentication is used, where a client certificate is used to authenticate the request.
    /// </summary>
    ClientCertificate
}
