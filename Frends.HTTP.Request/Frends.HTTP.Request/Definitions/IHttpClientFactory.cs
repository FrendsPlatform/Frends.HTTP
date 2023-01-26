using System.Net.Http;

namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Http Client Factory Interface
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Create client
    /// </summary>
    HttpClient CreateClient(Options options);
}
