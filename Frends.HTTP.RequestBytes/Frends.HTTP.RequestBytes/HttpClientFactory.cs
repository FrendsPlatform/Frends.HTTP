using System.Net.Http;
using Frends.HTTP.RequestBytes.Definitions;

namespace Frends.HTTP.RequestBytes;

/// <summary>
/// Default factory for creating HTTP clients.
/// </summary>
internal class HttpClientFactory : IHttpClientFactory
{
    /// <summary>
    /// Creates an HTTP client configured according to the options.
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
