using System.Net.Http;
using Frends.HTTP.Request.Definitions;

namespace Frends.HTTP.Request;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}
