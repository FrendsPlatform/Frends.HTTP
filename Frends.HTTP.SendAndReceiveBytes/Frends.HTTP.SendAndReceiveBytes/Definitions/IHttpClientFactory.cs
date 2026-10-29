using System.Net.Http;

namespace Frends.HTTP.SendAndReceiveBytes.Definitions;

/// <summary>
/// Http Client Factory Interface
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Create client
    /// </summary>
    /// <param name="options">Task options.</param>
    /// <returns>Configured HTTP client.</returns>
    HttpClient CreateClient(Options options);
}
