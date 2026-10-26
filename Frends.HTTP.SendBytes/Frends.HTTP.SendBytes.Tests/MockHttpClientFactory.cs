using System.Net.Http;
using Frends.HTTP.SendBytes.Definitions;
using RichardSzalay.MockHttp;

namespace Frends.HTTP.SendBytes.Tests;

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
