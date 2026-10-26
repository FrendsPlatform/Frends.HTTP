using System.Net.Http;
using Frends.HTTP.RequestBytes.Definitions;
using RichardSzalay.MockHttp;

namespace Frends.HTTP.RequestBytes.Tests;

public class MockHttpClientFactory : IHttpClientFactory
{
    private readonly MockHttpMessageHandler mockHttpMessageHandler;

    public MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler)
    {
        this.mockHttpMessageHandler = mockHttpMessageHandler;
    }

    public HttpClient CreateClient(Options options)
    {
        return mockHttpMessageHandler.ToHttpClient();
    }
}
