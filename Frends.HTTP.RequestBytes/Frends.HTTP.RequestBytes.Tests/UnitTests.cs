using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.RequestBytes.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using RichardSzalay.MockHttp;
using Assert = NUnit.Framework.Assert;
using Definitions = Frends.HTTP.RequestBytes.Definitions;
using Method = Frends.HTTP.RequestBytes.Definitions.Method;

namespace Frends.HTTP.RequestBytes.Tests;

[TestClass]
public class UnitTests
{
    private const string BasePath = "http://localhost:9191";
    private MockHttpMessageHandler mockHttpMessageHandler;

    [TestInitialize]
    public void TestInitialize()
    {
        mockHttpMessageHandler = new MockHttpMessageHandler();
        HTTP.ClearClientCache();
        HTTP.ClientFactory = new MockHttpClientFactory(mockHttpMessageHandler);
    }

    [TestCase(CertificateStoreLocation.CurrentUser, "current user")]
    [TestCase(CertificateStoreLocation.LocalMachine, "local machine")]
    public void CorrectStoreSearched(CertificateStoreLocation storeLocation, string storeLocationText)
    {
        var handler = new HttpClientHandler();
        var options = new Options
        {
            Authentication = Authentication.ClientCertificate,
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = storeLocation,
            CertificateThumbprint = "InvalidThumbprint",
        };
        var ex = Assert.Throws<FileNotFoundException>(() => handler.SetHandlerSettingsBasedOnOptions(options));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains(
            $"Certificate with thumbprint: 'INVALIDTHUMBPRINT' not found in {storeLocationText} cert store."));
    }

    [TestMethod]
    public async Task RequestShouldReuseCachedClientForSameOptions()
    {
        mockHttpMessageHandler.When("*").Respond(HttpStatusCode.OK);
        var input = GetInputParams(url: $"{BasePath}/anything");
        var opt = new Options
        {
            Authentication = Authentication.Basic,
            Username = "cache-test-username",
            Password = "cache-test-password",
            ProxyUsername = "cache-test-proxy-username",
            ProxyPassword = "cache-test-proxy-password",
            ClientCertificateKeyPhrase = "cache-test-certificate-key-phrase",
            ConnectionTimeoutSeconds = 60,
        };

        await HTTP.RequestBytes(input, opt, CancellationToken.None);
        await HTTP.RequestBytes(input, opt, CancellationToken.None);

        ClassicAssert.AreEqual(1, MemoryCache.Default.GetCount());

        foreach (var cacheEntry in MemoryCache.Default)
        {
            ClassicAssert.IsFalse(cacheEntry.Key.Contains("cache-test-username"));
            ClassicAssert.IsFalse(cacheEntry.Key.Contains("cache-test-password"));
            ClassicAssert.IsFalse(cacheEntry.Key.Contains("cache-test-proxy-username"));
            ClassicAssert.IsFalse(cacheEntry.Key.Contains("cache-test-proxy-password"));
            ClassicAssert.IsFalse(cacheEntry.Key.Contains("cache-test-certificate-key-phrase"));
        }

        opt.Password = "other";
        await HTTP.RequestBytes(input, opt, CancellationToken.None);

        ClassicAssert.AreEqual(2, MemoryCache.Default.GetCount());
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        var input = new Input
        {
            Method = Method.GET,
            Url = string.Empty,
            Headers = new Header[0],
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
        };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains("Url can not be empty."));
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
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.ClientCertificate,
            CertificateThumbprint = thumbprint,
        };

        HTTP.ClientFactory = new HttpClientFactory();

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task HttpRequestBytesReturnShoulReturnEmpty()
    {
        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        mockHttpMessageHandler.When(input.Url)
            .Respond("application/octet-stream", string.Empty);

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);
        ClassicAssert.AreEqual(0, result.BodySizeInMegaBytes);
        ClassicAssert.IsEmpty(result.Body);
    }

    [TestMethod]
    public async Task HttpRequestBytesShouldBeAbleToReturnBinary()
    {
        var testFileUriPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestData/frends_favicon.png");
        string localTestFilePath = new Uri(testFileUriPath).LocalPath;
        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        var actualFileBytes = File.ReadAllBytes(localTestFilePath);

        using var ms = new MemoryStream(actualFileBytes);
        mockHttpMessageHandler.When(input.Url)
            .Respond("image/png", ms);

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        ClassicAssert.NotNull(result.Body);
        ClassicAssert.AreEqual(actualFileBytes, result.Body);
    }

    [TestMethod]
    public async Task RequestTestGetWithParameters()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        var input = GetInputParams(url: "http://localhost:9191/endpoint?foo=bar&bar=foo");
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        var dict = new Dictionary<string, string>()
        {
            {
                "foo", "bar"
            },
            {
                "bar", "foo"
            },
        };

        mockHttpMessageHandler.When(input.Url).WithQueryString(dict)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public void RequestShuldThrowExceptionIfOptionIsSet()
    {
        const string expectedReturn = @"'FooBar'";

        mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);

        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
        };

        Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.RequestBytes(input, options, CancellationToken.None));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/octet-stream", "FooBar");

        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = false,
        };

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(expectedReturn, result.Body);
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
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.Basic,
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString(),
        };
        var sentAuthValue =
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", sentAuthValue)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual(expectedReturn, result.Body);
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
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken",
        };

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Bearer fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        var expectedReturn = Encoding.ASCII.GetBytes("FooBar");

        var input = new Input
        {
            Method = Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new[]
            {
                new Header()
                {
                    Name = "Authorization",
                    Value = "Basic fooToken",
                },
            },
            Message = string.Empty,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "barToken",
        };

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Basic fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual(expectedReturn, result.Body);
    }

    [TestMethod]
    public async Task PatchShouldComeThrough()
    {
        var message = "foo åäö";

        var input = new Input
        {
            Method = Method.PATCH,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[]
            {
            },
            Message = message,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        mockHttpMessageHandler.Expect(new HttpMethod("PATCH"), input.Url).WithContent(message)
            .Respond("application/octet-stream", message);

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual(message, Encoding.UTF8.GetString(result.Body));
    }

    [TestMethod]
    public async Task RequestShouldSetEncodingWithContentTypeCharsetIgnoringCase()
    {
        var codePageName = "iso-8859-1";
        var requestMessage = "åäö!";
        var expectedContentType = $"text/plain; charset={codePageName}";

        var contentType = new Header
        {
            Name = "cONTENT-tYpE",
            Value = expectedContentType,
        };
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[1]
            {
                contentType,
            },
            Message = requestMessage,
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        mockHttpMessageHandler.Expect(HttpMethod.Post, input.Url).WithHeaders("cONTENT-tYpE", expectedContentType)
            .WithContent(requestMessage)
            .Respond("application/octet-stream", "foo åäö");

        var result = (dynamic)await HTTP.RequestBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual("foo åäö", Encoding.UTF8.GetString(result.Body));
    }

    [TestMethod]
    public void HandlerShouldUseConfiguredProxy()
    {
        var handler = new HttpClientHandler();
        var options = new Options
        {
            UseProxy = true,
            ProxyUrl = "http://proxy.example.com:8080",
            ProxyUsername = "proxy-user",
            ProxyPassword = "proxy-password",
        };

        handler.SetHandlerSettingsBasedOnOptions(options);

        var proxy = handler.Proxy as WebProxy;
        Assert.That(proxy, Is.Not.Null);
        ClassicAssert.IsTrue(handler.UseProxy);
        ClassicAssert.AreEqual(new Uri("http://proxy.example.com:8080/"), proxy.Address);
        var credentials = proxy.Credentials.GetCredential(proxy.Address, string.Empty);
        ClassicAssert.AreEqual("proxy-user", credentials.UserName);
        ClassicAssert.AreEqual("proxy-password", credentials.Password);
    }

    [TestMethod]
    public async Task RequestShouldSetTls12And13WhenConfigured()
    {
        HTTP.ClientFactory = new HttpClientFactory();

        var input = GetInputParams(url: "https://httpbin.org/anything");
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            SslProtocolVersion = SslVersion.Tls12And13,
            HttpProtocolVersion = Definitions.HttpVersion.Http20,
        };

        var result = await HTTP.RequestBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(200, result.StatusCode);
    }

    private static Input GetInputParams(
        Method method = Method.GET,
        string url = BasePath,
        string message = "",
        params Header[] headers)
    {
        return new Input
        {
            Method = method,
            Url = url,
            Headers = headers,
            Message = message,
        };
    }
}
