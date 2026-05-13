using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HTTP.SendBytes.Definitions;

/// <summary>
/// Options class
/// </summary>
public class Options
{
    /// <summary>
    /// Method of authenticating request
    /// </summary>
    /// <example>OAuth</example>
    public Authentication Authentication { get; set; }

    /// <summary>
    /// If WindowsAuthentication is selected you should use domain\username
    /// </summary>
    /// <example>Username</example>
    [UIHint(nameof(Authentication), "", Authentication.WindowsAuthentication, Authentication.Basic)]
    public string Username { get; set; }

    /// <summary>
    /// Password for the user
    /// </summary>
    /// <example>Password123</example>
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", Authentication.WindowsAuthentication, Authentication.Basic)]
    public string Password { get; set; }

    /// <summary>
    /// Bearer token to be used for request. Token will be added as Authorization header.
    /// </summary>
    /// <example>Token123</example>
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", Authentication.OAuth)]
    public string Token { get; set; }

    /// <summary>
    /// Whether to route requests through a proxy server. When set to true, the ProxyUrl must be provided.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool UseProxy { get; set; } = false;

    /// <summary>
    /// Proxy URL used for the request.
    /// </summary>
    /// <example>http://proxy.example.com:8080</example>
    [UIHint(nameof(UseProxy), "", true)]
    public string ProxyUrl { get; set; }

    /// <summary>
    /// Username for proxy authentication. Leave empty if proxy does not require authentication.
    /// Both username and password must be provided together or left empty.
    /// </summary>
    /// <example>Username</example>
    [UIHint(nameof(UseProxy), "", true)]
    public string ProxyUsername { get; set; }

    /// <summary>
    /// Password for proxy authentication. Leave empty if proxy does not require authentication.
    /// Both username and password must be provided together or left empty.
    /// </summary>
    /// <example>Password123</example>
    [PasswordPropertyText]
    [UIHint(nameof(UseProxy), "", true)]
    public string ProxyPassword { get; set; }

    /// <summary>
    /// Specifies where the Client Certificate should be loaded from.
    /// </summary>
    /// <example>File</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    [DefaultValue(CertificateSource.CertificateStore)]
    public CertificateSource ClientCertificateSource { get; set; }

    /// <summary>
    /// Applicable only when Certificate Source is "File".
    /// Path to the Client Certificate, pfx (pkcs12) files are recommended. For other supported formats, see
    /// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2collection.import?view=netframework-4.7.1
    /// </summary>
    /// <example>domain\path</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateFilePath { get; set; }

    /// <summary>
    /// Applicable only when Certificate Source is "String".
    /// Client certificate bytes as a base64 encoded string, pfx (pkcs12) format is recommended. For other supported formats, see
    /// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2collection.import?view=netframework-4.7.1
    /// </summary>
    /// <example>domain\path</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateInBase64 { get; set; }

    /// <summary>
    /// Applicable only when Certificate Source is "File" or "String".
    /// Key phrase (password) to access the certificate data
    /// </summary>
    /// <example>string value</example>
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateKeyPhrase { get; set; }

    /// <summary>
    /// Applicable only when Certificate Source is "CertificateStore".
    /// Thumbprint for using client certificate authentication.
    /// </summary>
    /// <example>thumbprint</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string CertificateThumbprint { get; set; }

    /// <summary>
    /// Applicable only when Certificate Source is "CertificateStore".
    /// Store location for the certificate.
    /// </summary>
    /// <example>CurrentUser</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    [DefaultValue(CertificateStoreLocation.CurrentUser)]
    public CertificateStoreLocation CertificateStoreLocation { get; set; } = CertificateStoreLocation.CurrentUser;

    /// <summary>
    /// Should the entire certificate chain be loaded from the certificate store and included in the request. Only valid when using Certificate Store as the Certificate Source
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    [DefaultValue(true)]
    public bool LoadEntireChainForCertificate { get; set; }

    /// <summary>
    /// Timeout in seconds to be used for the connection and operation.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int ConnectionTimeoutSeconds { get; set; }

    /// <summary>
    /// If FollowRedirects is set to false, all responses with an HTTP status code from 300 to 399 is returned to the application.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool FollowRedirects { get; set; }

    /// <summary>
    /// Do not throw an exception on certificate error.
    /// </summary>
    /// <example>true</example>
    public bool AllowInvalidCertificate { get; set; }

    /// <summary>
    /// Some Api's return faulty content-type charset header. This setting overrides the returned charset.
    /// </summary>
    /// <example>true</example>
    public bool AllowInvalidResponseContentTypeCharSet { get; set; }

    /// <summary>
    /// Throw exception if return code of request is not successfull
    /// </summary>
    /// <example>true</example>
    public bool ThrowExceptionOnErrorResponse { get; set; }

    /// <summary>
    /// If set to false, cookies must be handled manually. Defaults to true.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool AutomaticCookieHandling { get; set; } = true;

    /// <summary>
    /// HTTP protocol version to use for requests. HTTP/1.1 is the default and most compatible option.
    /// </summary>
    /// <example>Http11</example>
    [DefaultValue(HttpVersion.Http11)]
    public HttpVersion HttpProtocolVersion { get; set; } = HttpVersion.Http11;

    /// <summary>
    /// SSL/TLS protocol version to use for secure connections. Default lets the OS decide, which matches previous behavior.
    /// Use Tls12 or Tls12And13 if the server requires a specific version.
    /// </summary>
    /// <example>Default</example>
    [DefaultValue(SslVersion.Default)]
    public SslVersion SslProtocolVersion { get; set; } = SslVersion.Default;
}
