using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.Request.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Method = Frends.HTTP.Request.Definitions;
using RichardSzalay.MockHttp;
using Assert = NUnit.Framework.Assert;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frends.HTTP.Request.Tests;

[TestClass]
public class UnitTests
{
    private const string BasePath = "http://localhost:9191";
    private MockHttpMessageHandler _mockHttpMessageHandler;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        HTTP.ClearClientCache();
        HTTP.ClientFactory = new MockHttpClientFactory(_mockHttpMessageHandler);
    }

    private Input GetInputParams(Method.Method method = Method.Method.GET, string url = BasePath, string message = "",
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
    public async Task RequestTestGetWithParameters()
    {
        const string expectedReturn = @"'FooBar'";
        var dict = new Dictionary<string, string>()
        {
            {"foo", "bar"},
            {"bar", "foo"}
        };

        _mockHttpMessageHandler.When($"{BasePath}/endpoint").WithQueryString(dict)
            .Respond("application/json", expectedReturn);

        var input = GetInputParams(url: "http://localhost:9191/endpoint?foo=bar&bar=foo");
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        var result = (dynamic)await HTTP.Request(input, options, CancellationToken.None);
        Console.WriteLine(result.Body.ToString());

        Assert.IsTrue(result.Body.Contains("FooBar"));
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        const string expectedReturn = @"'FooBar'";

        var input = new Input
        {
            Method = Method.Method.GET,
            Url = "",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains("Url can not be empty."));
    }

    [TestMethod]
    public void RequestShuldThrowExceptionIfOptionIsSet()
    {
        const string expectedReturn = @"'FooBar'";

        _mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);

        var input = new Input
        {
            Method = Method.Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = true };

        var ex = Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains("FooBar"));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        const string expectedReturn = @"'FooBar'";

        _mockHttpMessageHandler.When($"{BasePath}/endpoint")
            .Respond(HttpStatusCode.InternalServerError, "application/json", expectedReturn);


        var input = new Input
        {
            Method = Method.Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options { ConnectionTimeoutSeconds = 60, ThrowExceptionOnErrorResponse = false };

        var result = (dynamic)await HTTP.Request(input, options, CancellationToken.None);

        Assert.IsTrue(result.Body.Contains("FooBar"));
    }

    [TestMethod]
    public async Task RequestShouldAddBasicAuthHeaders()
    {
        const string expectedReturn = @"'FooBar'";

        var input = new Input
        {
            Method = Method.Method.GET,
            Url = " http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = ""
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

        _mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", sentAuthValue)
            .Respond("application/json", expectedReturn);

        var result = (dynamic)await HTTP.Request(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public async Task RequestShouldAddOAuthBearerHeader()
    {
        const string expectedReturn = @"'FooBar'";

        var input = new Input
        {
            Method = Method.Method.GET,
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

        _mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Bearer fooToken")
            .Respond("application/json", expectedReturn);
        await HTTP.Request(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }
    
    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        const string expectedReturn = @"'FooBar'";

        var input = new Input
        {
            Method = Method.Method.GET,
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

        _mockHttpMessageHandler.Expect($"{BasePath}/endpoint").WithHeaders("Authorization", "Basic fooToken")
            .Respond("application/json", expectedReturn);
        await HTTP.Request(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [TestMethod]
    public void RequestShouldAddClientCertificate()
    {
        const string thumbprint = "ABCD";
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ResultMethod.REST
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
            await HTTP.Request(input, options, CancellationToken.None));

        Assert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task RestRequestBodyReturnShouldBeOfTypeJToken()
    {
        dynamic dyn = new
        {
            Foo = "Bar"
        };
        string output = JsonConvert.SerializeObject(dyn);


        var input = new Input
        { Method = Method.Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "", ResultMethod = ResultMethod.REST };
        var options = new Options
        { ConnectionTimeoutSeconds = 60, Authentication = Authentication.OAuth, Token = "fooToken" };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("application/json", output);

        var result = await HTTP.Request(input, options, CancellationToken.None);
        var resultBody = result.Body as JToken;
        Assert.AreEqual(new JValue("Bar"), resultBody["Foo"]);
    }
    
    [TestMethod]
    public async Task RestRequestShouldNotThrowIfReturnIsEmpty()
    {
        var input = new Input
        { Method = Method.Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "", ResultMethod = ResultMethod.REST };
        var options = new Options
        { ConnectionTimeoutSeconds = 60, Authentication = Authentication.OAuth, Token = "fooToken" };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("application/json", String.Empty);

        var result = await HTTP.Request(input, options, CancellationToken.None);

        Assert.AreEqual(new JValue(""), result.Body);
    }
    
    [TestMethod]
    public void RestRequestShouldThrowIfReturnIsNotValidJson()
    {
        var input = new Input
        { Method = Method.Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "", ResultMethod = ResultMethod.REST };
        var options = new Options
        { ConnectionTimeoutSeconds = 60, Authentication = Authentication.OAuth, Token = "fooToken" };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("application/json", "<fail>failbar<fail>");
        var ex = Assert.ThrowsAsync<JsonReaderException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        Assert.AreEqual("Unable to read response message as json: <fail>failbar<fail>", ex.Message);
    }
    
    [TestMethod]
    public async Task HttpRequestBodyReturnShouldBeOfTypeString()
    {
        const string expectedReturn = "<foo>BAR</foo>";

        var input = new Input
        { Method = Method.Method.GET, Url = "http://localhost:9191/endpoint", Headers = new Header[0], Message = "", ResultMethod = ResultMethod.HTTP };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.When(input.Url)
            .Respond("text/plain", expectedReturn);
        var result = await HTTP.Request(input, options, CancellationToken.None);

        Assert.AreEqual(expectedReturn, result.Body);
    }
    
    [TestMethod]
    public async Task PatchShouldComeThroug()
    {
        var message = "åäö";

        var input = new Input
        {
            Method = Method.Method.PATCH,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[] { },
            Message = message,
            ResultMethod = ResultMethod.HTTP
        };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.Expect(new HttpMethod("PATCH"), input.Url).WithContent(message)
            .Respond("text/plain", "foo åäö");

        await HTTP.Request(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
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
            Method = Method.Method.POST,
            Url = "http://localhost:9191/endpoint",
            Headers = new Header[1] { contentType },
            Message = requestMessage,
            ResultMethod = ResultMethod.HTTP
        };
        var options = new Options { ConnectionTimeoutSeconds = 60 };

        _mockHttpMessageHandler.Expect(HttpMethod.Post, input.Url).WithHeaders("cONTENT-tYpE", expectedContentType).WithContent(requestMessage)
            .Respond("text/plain", "foo åäö");

        await HTTP.Request(input, options, CancellationToken.None);

        _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
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
