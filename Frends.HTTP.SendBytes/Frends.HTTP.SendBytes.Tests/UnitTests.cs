using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using Assert = NUnit.Framework.Assert;
using Frends.HTTP.SendBytes.Definitions;
using System.Collections.Generic;
using System.Text;
using Method = Frends.HTTP.SendBytes.Definitions.Method;
using System.Net;

namespace Frends.HTTP.SendBytes.Tests;

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

    private static Input GetInputParams(Method method = Method.POST, string url = _basePath, byte[] message = null, params Header[] headers)
    {
        return new Input
        {
            Method = method,
            Url = url,
            Headers = headers,
            ContentBytes = new byte[0]
        };
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = "",
            Headers = new Header[0],
            ContentBytes = new byte[0]
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.SendBytes(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains("Url can not be empty."));
    }

    [TestMethod]
    public void RequestShouldAddClientCertificate()
    {
        const string thumbprint = "ABCD";
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0]
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
            await HTTP.SendBytes(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task HttpSendBytesReturnShoulReturnEmpty()
    {
        var input = new Input { Method = Method.POST, Url = "http://localhost:9191/endpoint", Headers = new Header[0], ContentBytes = new byte[0] };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("application/octet-stream", String.Empty);

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);
        Assert.IsEmpty(result.Body);
    }

    [TestMethod]
    public async Task RequestTestPostWithParameters()
    {
        var input = GetInputParams(url: "http://localhost:9191/endpoint?foo=bar&bar=foo");
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        var dict = new Dictionary<string, string>()
        {
            {"foo", "bar"},
            {"bar", "foo"}
        };

        _mockHttpMessageHandler.When(input.Url).WithQueryString(dict)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        Assert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfOptionIsSet()
    {
        const string expectedReturn = @"'FooBar'";

        _mockHttpMessageHandler.When($"{_basePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);

        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0]
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.SendBytes(input, options, CancellationToken.None));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        _mockHttpMessageHandler.When($"{_basePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/octet-stream", "FooBar");

        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0]
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = false };

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        Assert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task RequestShouldAddBasicAuthHeaders()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = " http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0]
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.Basic,
            Username = "Foo",
            Password = "Bar"
        };
        var sentAuthValue =
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", sentAuthValue)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task RequestShouldAddOAuthBearerHeader()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0]
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", "Bearer fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        var input = new Input
        {
            Method = Method.PUT,
            Url = "http://localhost:9191/endpoint",
            Headers = new[] { new Header() { Name = "Authorization", Value = "Basic fooToken" } },
            ContentBytes = new byte[0]
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "barToken"
        };

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", "Basic fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task PatchShouldComeThrough()
    {
        var expectedString = "Tää on se odotettu stringi!öÖööÄ";
        var bytes = Encoding.UTF8.GetBytes(expectedString);
        var input = new Input
        {
            Method = Method.PATCH,
            Url = "http://localhost:9191/data",
            Headers = new[]
            {
                    new Header {Name = "Content-Type", Value = "text/plain; charset=utf-8"},
                    new Header() {Name = "Content-Length", Value = bytes.Length.ToString()}
                },
            ContentBytes = bytes
        };

        var options = new Options
        { ConnectionTimeoutSeconds = 60, Authentication = Authentication.OAuth, Token = "fooToken" };

        _mockHttpMessageHandler.Expect(HttpMethod.Patch, input.Url).WithHeaders("Content-Type", "text/plain; charset=utf-8").WithContent(expectedString)
            .Respond("text/plain", "foo åäö");

        var result = (Result)await HTTP.SendBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual("foo åäö", result.Body);
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
