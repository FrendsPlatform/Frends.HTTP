using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Frends.HTTP.UploadFile.Definitions
{
    /// <summary>
    /// Options for the HTTP request.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Method of authenticating request
        /// </summary>
        /// <example>Basic</example>
        public Authentication Authentication { get; set; }

        /// <summary>
        /// If WindowsAuthentication is selected you should use domain\username
        /// </summary>
        /// <example>Domain\User</example>
        [UIHint(nameof(Definitions.Authentication), "", Authentication.WindowsAuthentication, Authentication.Basic)]
        public string Username { get; set; }

        /// <summary>
        /// Password for the user.
        /// </summary>
        /// <example>Example Password</example>
        [PasswordPropertyText]
        [UIHint(nameof(Definitions.Authentication), "", Authentication.WindowsAuthentication, Authentication.Basic)]
        public string Password { get; set; }

        /// <summary>
        /// Bearer token to be used for request. Token will be added as Authorization header.
        /// </summary>
        /// <example>exampleOAuthToken</example>
        [PasswordPropertyText]
        [UIHint(nameof(Definitions.Authentication), "", Authentication.OAuth)]
        public string Token { get; set; }

        /// <summary>
        /// Thumbprint for using client certificate authentication.
        /// </summary>
        /// <example>exampleCertificateThumbprint</example>
        [UIHint(nameof(Definitions.Authentication), "", Authentication.ClientCertificate)]
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Timeout in seconds to be used for the connection and operation.
        /// </summary>
        /// <example>30</example>
        [DefaultValue(30)]
        public int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// If FollowRedirects is set to false, all responses with an HTTP status code from 300 to 399 is returned to the application.
        /// </summary>
        /// /// <example>true</example>
        [DefaultValue(true)]
        public bool FollowRedirects { get; set; }

        /// <summary>
        /// Do not throw an exception on certificate error.
        /// </summary>
        /// <example>true</example>
        public bool AllowInvalidCertificate { get; set; }

        /// <summary>
        /// Some API's return faulty content-type charset header. This setting overrides the returned charset.
        /// </summary>
        /// <example>false</example>
        public bool AllowInvalidResponseContentTypeCharSet { get; set; }

        /// <summary>
        /// Throw exception if return code of request is not successfull
        /// </summary>
        /// <example>true</example> 
        public bool ThrowExceptionOnErrorResponse { get; set; }
    }
}