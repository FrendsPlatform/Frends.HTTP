using System.Net.Http;
using Frends.HTTP.SendBytes.Definitions;

namespace Frends.HTTP.SendBytes;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}
