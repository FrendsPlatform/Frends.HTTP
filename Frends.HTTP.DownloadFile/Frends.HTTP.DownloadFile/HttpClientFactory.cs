using Frends.HTTP.DownloadFile.Definitions;
using System.Net.Http;

namespace Frends.HTTP.DownloadFile;

internal class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(Options options)
    {
        var handler = new HttpClientHandler();
        handler.SetHandlerSettingsBasedOnOptions(options);
        return new HttpClient(handler);
    }
}