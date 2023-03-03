namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Certificate source.
/// </summary>
public enum CertificateSource
{
    /// <summary>
    /// Source is certificate store.
    /// </summary>
    CertificateStore,
    /// <summary>
    /// Source is a file.
    /// </summary>
    File,
    /// <summary>
    /// Source is a string.
    /// </summary>
    String
}

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
