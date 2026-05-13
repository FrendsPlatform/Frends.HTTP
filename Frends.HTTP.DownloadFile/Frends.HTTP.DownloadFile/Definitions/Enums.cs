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


/// <summary>
/// HTTP protocol version used for requests.
/// </summary>
public enum HttpVersion
{
    /// <summary>
    /// HTTP/1.1 - default, widely supported.
    /// </summary>
    Http11,
    /// <summary>
    /// HTTP/2 - multiplexed, requires server support. 
    /// If the server does not support HTTP/2 via ALPN, the request will fail with an exception instead of falling back to HTTP/1.1.
    /// </summary>
    Http20
}

/// <summary>
/// SSL/TLS protocol version used for secure connections.
/// </summary>
public enum SslVersion
{
    /// <summary>
    /// OS decides the protocol version.
    /// </summary>
    Default,
    /// <summary>
    /// TLS 1.2 only.
    /// </summary>
    Tls12,
    /// <summary>
    /// TLS 1.3 only.
    /// </summary>
    Tls13,
    /// <summary>
    /// TLS 1.2 and TLS 1.3 - use when server compatibility is uncertain.
    /// </summary>
    Tls12And13
}
