using System.Net.Http;
using Frends.HTTP.SendAndReceiveBytes.Definitions;

namespace Frends.HTTP.SendAndReceiveBytes;

/// <summary>
/// Default factory creating HTTP clients based on options.
/// </summary>
internal class HttpClientFactory : IHttpClientFactory
{
    /// <summary>
    /// Create client.
    /// </summary>
    /// <param name="options">Task options.</param>
    /// <returns>Configured HTTP client.</returns>
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}
