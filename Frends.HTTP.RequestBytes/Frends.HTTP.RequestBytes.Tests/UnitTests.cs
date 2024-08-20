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

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);
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

        using var ms = new MemoryStream(actualFileBytes);
        _mockHttpMessageHandler.When(input.Url)
            .Respond("image/png", ms);

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
    public async Task RequestTestGetWithContent()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("OK");


        var contentType = new Header { Name = "Content-Type", Value = "text/plain" };
        var input = GetInputParams(
            url: "http://localhost:9191/endpoint",
            method: Method.GET,
            headers: new Header[1] { contentType },
            message: "test"
        );
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.When(input.Url).WithHeaders("Content-Type", "text/plain").WithPartialContent("test")
            .Respond("application/octet-stream", "OK");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);
        Assert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfOptionIsSet()
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

    [TestMethod]
    public async Task RequestShouldAddBasicAuthHeaders()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        var input = new Input
        {
            Method = Method.GET,
            Url = " http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.Basic,
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        var sentAuthValue =
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", sentAuthValue)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public async Task RequestShouldAddOAuthBearerHeader()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

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
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", "Bearer fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new[] { new Header() { Name = "Authorization", Value = "Basic fooToken" } },
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "barToken"
        };

        _mockHttpMessageHandler.Expect($"{_basePath}/endpoint").WithHeaders("Authorization", "Basic fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public async Task PatchShouldComeThrough()
    {
        var message = "foo åäö";

        var input = new Input
        {
            Method = Method.PATCH,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[] { },
            Message = message
        };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.Expect(new HttpMethod("PATCH"), input.Url).WithContent(message)
            .Respond("application/octet-stream", message);

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual(message, Encoding.UTF8.GetString(result.Body));
    }

    [TestMethod]
    public async Task RequestShouldSetEncodingWithContentTypeCharsetIgnoringCase()
    {
        var codePageName = "iso-8859-1";
        var requestMessage = "åäö!";
        var expectedContentType = $"text/plain; charset={codePageName}";

        var contentType = new Header { Name = "cONTENT-tYpE", Value = expectedContentType };
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[1] { contentType },
            Message = requestMessage
        };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.Expect(HttpMethod.Post, input.Url).WithHeaders("cONTENT-tYpE", expectedContentType).WithContent(requestMessage)
            .Respond("application/octet-stream", "foo åäö");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.AreEqual("foo åäö", Encoding.UTF8.GetString(result.Body));
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
