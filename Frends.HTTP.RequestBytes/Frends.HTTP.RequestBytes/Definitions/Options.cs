using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HTTP.RequestBytes.Definitions;

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
    /// Specifies where the Client Certificate should be loaded from.
    /// </summary>
    /// <example>File</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    [DefaultValue(CertificateSource.CertificateStore)]
    public CertificateSource ClientCertificateSource { get; set; }

    /// <summary>
    /// Path to the Client Certificate when using a file as the Certificate Source, pfx (pkcs12) files are recommended. For other supported formats, see
    /// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2collection.import?view=netframework-4.7.1
    /// </summary>
    /// <example>domain\path</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateFilePath { get; set; }

    /// <summary>
    /// Client certificate bytes as a base64 encoded string when using a string as the Certificate Source , pfx (pkcs12) format is recommended. For other supported formates, see
    /// https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2collection.import?view=netframework-4.7.1
    /// </summary>
    /// <example>domain\path</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateInBase64 { get; set; }

    /// <summary>
    /// Key phrase (password) to access the certificate data when using a string or file as the Certificate Source
    /// </summary>
    /// <example>string value</example>
    [PasswordPropertyText]
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string ClientCertificateKeyPhrase { get; set; }

    /// <summary>
    /// Thumbprint for using client certificate authentication.
    /// </summary>
    /// <example>thumbprint</example>
    [UIHint(nameof(Authentication), "", Authentication.ClientCertificate)]
    public string CertificateThumbprint { get; set; }

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
}
