using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using Assert = NUnit.Framework.Assert;
using Frends.HTTP.RequestBytes.Definitions;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using System.Xml.Linq;
using Method = Frends.HTTP.RequestBytes.Definitions.Method;
using System.Net;

namespace Frends.HTTP.RequestBytes.Tests;

[TestClass]
public class UnitTests
{
    private const string _basePath = "http://localhost:9191";
    private MockHttpMessageHandler _mockHttpMessageHandler;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        HTTP.ClearClientCache();
        HTTP.ClientFactory = new MockHttpClientFactory(_mockHttpMessageHandler);
    }

    private static Input GetInputParams(Method method = Method.GET, string url = _basePath, string message = "",
        params Header[] headers)
    {
        return new Input
        {
            Method = method,
            Url = url,
            Headers = headers,
            Message = message
        };
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        var input = new Input
        {
            Method = Method.GET,
            Url = "",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains("Url can not be empty."));
    }

    [TestMethod]
    public void RequestShouldAddClientCertificate()
    {
        const string thumbprint = "ABCD";
        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.ClientCertificate,
            CertificateThumbprint = thumbprint
        };

        HTTP.ClientFactory = new HttpClientFactory();

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task HttpRequestBytesReturnShoulReturnEmpty()
    {
        var input = new Input { Method = Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "" };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("application/octet-stream", String.Empty);

        var result = (dynamic) await HTTP.RequestBytes(input, options, CancellationToken.None);
        Assert.AreEqual(0, result.BodySizeInMegaBytes);
        Assert.IsEmpty(result.Body);
    }

    [TestMethod]
    public async Task HttpRequestBytesShouldBeAbleToReturnBinary()
    {
        var testFileUriPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestData/frends_favicon.png");
        string localTestFilePath = new Uri(testFileUriPath).LocalPath;
        var input = new Input
        { Method = Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "" };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        var actualFileBytes = File.ReadAllBytes(localTestFilePath);
        _mockHttpMessageHandler.When(input.Url)
            .Respond("image/png", new MemoryStream(actualFileBytes));

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        Assert.NotNull(result.Body);
        Assert.AreEqual(actualFileBytes, result.Body);
    }

    [TestMethod]
    public async Task RequestTestGetWithParameters()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        var input = GetInputParams(url: "http://localhost:9191/endpoint?foo=bar&bar=foo");
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        var dict = new Dictionary<string, string>()
        {
            {"foo", "bar"},
            {"bar", "foo"}
        };

        _mockHttpMessageHandler.When(input.Url).WithQueryString(dict)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        Assert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public void RequestShuldThrowExceptionIfOptionIsSet()
    {
        const string expectedReturn = @"'FooBar'";

        _mockHttpMessageHandler.When($"{_basePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);

        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        _mockHttpMessageHandler.When($"{_basePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/octet-stream", "FooBar");


        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = false };

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        Assert.AreEqual(expectedReturn, result.Body);
    }
}

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
