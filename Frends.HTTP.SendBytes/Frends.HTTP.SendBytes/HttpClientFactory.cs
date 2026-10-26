using System.Net.Http;
using Frends.HTTP.SendBytes.Definitions;

namespace Frends.HTTP.SendBytes;

/// <summary>
/// Default factory for creating HTTP clients.
/// </summary>
internal class HttpClientFactory : IHttpClientFactory
{
    /// <summary>
    /// Creates an HTTP client based on the given options.
    /// </summary>
    /// <param name="options">Additional parameters.</param>
    /// <returns>Configured HTTP client.</returns>
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}
