using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.Request.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Method = Frends.HTTP.Request.Definitions;
using Assert = NUnit.Framework.Assert;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Frends.HTTP.Request.Tests;

[TestClass]
public class UnitTests
{
    private const string BasePath = "https://httpbin.org";

    [TestInitialize]
    public void TestInitialize()
    {
        HTTP.ClearClientCache();
    }

    private static Input GetInputParams(Method.Method method = Method.Method.GET, string url = BasePath,
        string message = "",
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
        var expected = "\"args\": {\n    \"id\": \"2\", \n    \"userId\": \"1\"\n  }";
        var input = GetInputParams(url: $"{BasePath}/anything?id=2&userId=1");
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Body.Contains(expected));
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfUrlEmpty()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = "",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true
        };

        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains("Url can not be empty."));
    }

    [TestMethod]
    public void RequestShouldThrowExceptionIfOptionIsSet()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/invalid",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true
        };

        var ex = Assert.ThrowsAsync<WebException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(
            ex.Message.Contains("Request to 'https://httpbin.org/invalid' failed with status code 404"));
    }

    [TestMethod]
    public async Task RequestShouldNotThrowIfOptionIsNotSet()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/invalid",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = false
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(
            result.Body.Contains("404 Not Found"));
    }

    [TestMethod]
    public async Task RequestShouldAddBasicAuthHeaders()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/anything",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.Basic,
            Username = "username",
            Password = "password",
        };
        var result = await HTTP.Request(input, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Body.Contains("\"Authorization\": \"Basic dXNlcm5hbWU6cGFzc3dvcmQ=\""));
    }

    [TestMethod]
    public async Task RequestShouldAddOAuthBearerHeader()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/anything",
            Headers = new Header[0],
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Body.Contains("\"Authorization\": \"Bearer fooToken\""));
    }

    [TestMethod]
    public async Task AuthorizationHeaderShouldOverrideOption()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/anything",
            Headers = new[]
            {
                new Header()
                {
                    Name = "Authorization",
                    Value = "Basic fooToken"
                }
            },
            Message = ""
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "barToken"
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Body.Contains("\"Authorization\": \"Basic fooToken\""));
    }

    [TestMethod]
    public void RequestShouldAddClientCertificate()
    {
        const string thumbprint = "ABCD";
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = BasePath,
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = true,
            Authentication = Authentication.ClientCertificate,
            CertificateThumbprint = thumbprint
        };

        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await HTTP.Request(input, options, CancellationToken.None));

        ClassicAssert.IsTrue(ex.Message.Contains($"Certificate with thumbprint: '{thumbprint}' not"));
    }

    [TestMethod]
    public async Task RestRequestBodyReturnShouldBeOfTypeJToken()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/anything",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);
        var resultBody = (JToken)result.Body;
        ClassicAssert.AreEqual(new JValue("GET"), resultBody["method"]);
    }

    [TestMethod]
    public async Task RestRequestShouldNotThrowIfReturnIsEmpty()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/status/200",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(new JValue(""), result.Body);
    }

    [TestMethod]
    public async Task RestRequest_NonJsonResponse_ShouldReturnRawStringInsteadOfThrowing()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/xml",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            Authentication = Authentication.OAuth,
            Token = "fooToken"
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsInstanceOf<string>(result.Body);
        ClassicAssert.IsTrue(((string)result.Body).Contains("<!--"));
        ClassicAssert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public async Task HttpRequestBodyReturnShouldBeOfTypeString()
    {
        const string expectedReturn = "<!--  A SAMPLE set of slides  -->";

        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/xml",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.String
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        Assert.That(result.Body.Contains(expectedReturn), result.Body);
    }

    [TestMethod]
    public async Task PatchShouldComeThrough()
    {
        var message = "åäö";

        var input = new Input
        {
            Method = Method.Method.PATCH,
            Url = $"{BasePath}/anything",
            Headers = new Header[]
            {
            },
            Message = message,
            ResultMethod = ReturnFormat.String
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Body.Contains("\"data\": \"\\u00e5\\u00e4\\u00f6\""), result.Body);
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
            Value = expectedContentType
        };
        var input = new Input
        {
            Method = Method.Method.POST,
            Url = $"{BasePath}/anything",
            Headers = new Header[1]
            {
                contentType
            },
            Message = requestMessage,
            ResultMethod = ReturnFormat.String
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsTrue(result.Body.Contains("\"Content-Type\": \"text/plain; charset=iso-8859-1\""));
    }

    [TestMethod]
    public async Task RequestTest_GetMethod_ShouldSendEmptyContent()
    {
        var input = GetInputParams(
            url: $"{BasePath}/anything",
            method: Method.Method.GET,
            message: "This should not be sent"
        );

        var options = new Options
        {
            ConnectionTimeoutSeconds = 60
        };
        var result = await HTTP.Request(input, options, CancellationToken.None);

        Assert.That(result.Body.Contains("\"data\": \"\""), result.Body);
    }

    [TestCase(CertificateStoreLocation.CurrentUser, "current user")]
    [TestCase(CertificateStoreLocation.LocalMachine, "local machine")]
    public void CorrectStoreSearched(CertificateStoreLocation storeLocation, string storeLocationText)
    {
        var handler = new HttpClientHandler();
        X509Certificate2[] certificates = Array.Empty<X509Certificate2>();
        var options = new Options
        {
            Authentication = Authentication.ClientCertificate,
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = storeLocation,
            CertificateThumbprint = "InvalidThumbprint",
        };
        var ex = Assert.Throws<FileNotFoundException>(() =>
            handler.SetHandlerSettingsBasedOnOptions(options, ref certificates));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message.Contains(
            $"Certificate with thumbprint: 'INVALIDTHUMBPRINT' not found in {storeLocationText} cert store."));
    }

    [TestMethod]
    public async Task RestRequest_NonJsonErrorResponse_ShouldReturnRawStringWithStatusCode()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/html",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            ThrowExceptionOnErrorResponse = false
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.AreEqual(200, result.StatusCode);
        ClassicAssert.IsInstanceOf<string>(result.Body);
        ClassicAssert.IsTrue(((string)result.Body).Contains("<html"));
    }

    [TestMethod]
    public async Task RestRequest_JsonContentType_ShouldStillReturnJToken()
    {
        var input = new Input
        {
            Method = Method.Method.GET,
            Url = $"{BasePath}/json",
            Headers = new Header[0],
            Message = "",
            ResultMethod = ReturnFormat.JToken
        };
        var options = new Options
        {
            ConnectionTimeoutSeconds = 60
        };

        var result = await HTTP.Request(input, options, CancellationToken.None);

        ClassicAssert.IsInstanceOf<JToken>(result.Body);
        ClassicAssert.AreEqual(200, result.StatusCode);
    }
}
