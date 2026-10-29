using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.SendBytes.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using RichardSzalay.MockHttp;
using Assert = NUnit.Framework.Assert;
using Definitions = Frends.HTTP.SendBytes.Definitions;
using Method = Frends.HTTP.SendBytes.Definitions.Method;

namespace Frends.HTTP.SendBytes.Tests;

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

        await HTTP.SendBytes(input, opt, CancellationToken.None);
        await HTTP.SendBytes(input, opt, CancellationToken.None);

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
        await HTTP.SendBytes(input, opt, CancellationToken.None);

        ClassicAssert.AreEqual(2, MemoryCache.Default.GetCount());
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = string.Empty,
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
        };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.SendBytes(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains("Url can not be empty."));
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
            ContentBytes = new byte[0],
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
            await HTTP.SendBytes(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task HttpSendBytesReturnShoulReturnEmpty()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        mockHttpMessageHandler.When(input.Url)
            .Respond("application/octet-stream", string.Empty);

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);
        ClassicAssert.IsEmpty(result.Body);
    }

    [TestMethod]
    public async Task RequestTestPostWithParameters()
    {
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

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfOptionIsSet()
    {
        const string expectedReturn = @"'FooBar'";

        mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);

        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
        };

        Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.SendBytes(input, options, CancellationToken.None));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/octet-stream", "FooBar");

        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = false,
        };

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task RequestShouldAddBasicAuthHeaders()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = " http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.Basic,
            Username = "Foo",
            Password = "Bar",
        };
        var sentAuthValue =
            "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", sentAuthValue)
            .Respond("application/octet-stream", "FooBar");

        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task RequestShouldAddOAuthBearerHeader()
    {
        var input = new Input
        {
            Method = Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken",
        };

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Bearer fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual("FooBar", result.Body);
    }

    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        var input = new Input
        {
            Method = Method.PUT,
            Url = "http://localhost:9191/endpoint",
            Headers = new[]
            {
                new Header()
                {
                    Name = "Authorization",
                    Value = "Basic fooToken",
                },
            },
            ContentBytes = new byte[0],
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "barToken",
        };

        mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Basic fooToken")
            .Respond("application/octet-stream", "FooBar");
        var result = (dynamic)await HTTP.SendBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual("FooBar", result.Body);
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
                new Header
                {
                    Name = "Content-Type",
                    Value = "text/plain; charset=utf-8",
                },
                new Header()
                {
                    Name = "Content-Length",
                    Value = bytes.Length.ToString(),
                },
            },
            ContentBytes = bytes,
        };

        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken",
        };

        mockHttpMessageHandler.Expect(HttpMethod.Patch, input.Url)
            .WithHeaders("Content-Type", "text/plain; charset=utf-8").WithContent(expectedString)
            .Respond("text/plain", "foo åäö");

        var result = (Result)await HTTP.SendBytes(input, options, CancellationToken.None);

        mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        ClassicAssert.AreEqual("foo åäö", result.Body);
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

        var input = new Input
        {
            Method = Method.POST,
            Url = "https://httpbin.org/anything",
            Headers = Array.Empty<Header>(),
            ContentBytes = Encoding.UTF8.GetBytes("test"),
        };

        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            SslProtocolVersion = SslVersion.Tls12And13,
            HttpProtocolVersion = Definitions.HttpVersion.Http20,
        };

        var result = await HTTP.SendBytes(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(200, result.StatusCode);
    }

    private static Input GetInputParams(
        Method method = Method.POST,
        string url = BasePath,
        byte[] message = null,
        params Header[] headers)
    {
        return new Input
        {
            Method = method,
            Url = url,
            Headers = headers,
            ContentBytes = new byte[0],
        };
    }
}
