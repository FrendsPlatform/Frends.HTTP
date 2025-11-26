using System.Net.Http;
using Frends.HTTP.DownloadFile.Definitions;
using RichardSzalay.MockHttp;

namespace Frends.HTTP.DownloadFile.Tests;

public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler;

    public MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler)
    {
        _mockHttpMessageHandler = mockHttpMessageHandler;
    }
    public HttpClient CreateClient(Options options)
    {
        return _mockHttpMessageHandler.ToHttpClient();
    }
}
