using System.Net.Http;

namespace Frends.HTTP.SendBytes.Definitions;

/// <summary>
/// Http Client Factory Interface
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Create client
    /// </summary>
    /// <param name="options">Additional parameters.</param>
    /// <returns>Configured HTTP client.</returns>
    HttpClient CreateClient(Options options);
}
