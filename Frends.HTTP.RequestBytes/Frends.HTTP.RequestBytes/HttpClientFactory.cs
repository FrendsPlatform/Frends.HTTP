using System.Net.Http;
using Frends.HTTP.RequestBytes.Definitions;

namespace Frends.HTTP.RequestBytes;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}
