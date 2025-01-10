using Frends.HTTP.UploadFile.Definitions;
using HttpMock;
using HttpMock.Verify.NUnit;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HTTP.UploadFile.Tests;

[TestFixture]
public class IntegrationTest
{
    private IHttpServer _stubHttp;
    private static readonly string testFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..", "Test_files", "test_file.txt"));
    private static readonly string testCodePageName = "iso-8859-1";
    private static readonly string expectedContentType = $"text/plain; charset={testCodePageName}";
    private static readonly Header contentTypeHeader = new() { Name = "cONtENT-tYpE", Value = expectedContentType };

    private static readonly Input defaultInput = new()
    {
        Method = Method.POST,
        Url = "http://localhost:9191/endpoint",
        Headers = new Header[1] { contentTypeHeader },
        FilePath = testFilePath
    };


    [SetUp]
    public void Setup()
    {
        _stubHttp = HttpMockRepository.At("http://localhost:9191");
        _stubHttp.Stub(x => x.Post("/endpoint"))
            .AsContentType($"text/plain; charset={testCodePageName}")
            .Return("foo ���")
            .OK();
        _stubHttp.Stub(x => x.Post("/wrong-endpoint"))
            .AsContentType($"text/plain")
            .Return("error occured")
            .NotFound();
    }

    [Test]
    public async Task RequestShouldSetEncodingWithContentTypeCharsetIgnoringCase()
    {
        var utf8ByteArray = File.ReadAllBytes(testFilePath);

        var result = await UploadFileTask.UploadFile(defaultInput, new Options(), CancellationToken.None);

        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
        var requestHead = request.RequestHead;
        var requestBodyByteArray = Encoding.GetEncoding(testCodePageName).GetBytes(request.Body);
        var requestContentType = requestHead.Headers["cONTENT-tYpE"];

        Assert.That(requestContentType, Is.EqualTo(expectedContentType));
        Assert.That(requestBodyByteArray, Is.EqualTo(utf8ByteArray));
    }

    [Test]
    public void ThrowOnErrorResponse()
    {
        var input = defaultInput;
        input.Url = "http://localhost:9191/wrong-endpoint";
        var options = new Options
        {
            ThrowExceptionOnErrorResponse = true
        };

        Assert.ThrowsAsync<WebException>(() => UploadFileTask.UploadFile(input, options, CancellationToken.None));
    }

    [Test]
    public async Task AddBasicAuthHeader()
    {
        var options = new Options
        {
            Authentication = Authentication.Basic,
            Username = "foo",
            Password = "bar",
        };
        var encodedValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));

        var result = await UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None);
        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
        Assert.That(request.RequestHead.Headers["Authorization"], Is.EqualTo($"Basic {encodedValue}"));
    }

    [Test]
    public async Task AddOAuthHeader()
    {
        var options = new Options
        {
            Authentication = Authentication.OAuth,
            Token = "foobar"
        };

        var result = await UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None);

        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
        Assert.That(request.RequestHead.Headers["Authorization"], Is.EqualTo($"Bearer foobar"));
    }

    [Test]
    public async Task AddInvalidHeader()
    {
        Header invalidHeader = new() { Name = string.Empty, Value = "bar" };
        var input = defaultInput;
        input.Headers = new Header[2] { contentTypeHeader, invalidHeader };

        var result = await UploadFileTask.UploadFile(input, new Options(), CancellationToken.None);
        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
        var isInvalidAdded = request.RequestHead.Headers.Any(x => x.Key == string.Empty);
        Assert.IsFalse(isInvalidAdded);
    }

    [Test]
    public async Task AddWindowsIntegratedSecurity()
    {
        var options = new Options
        {
            Authentication = Authentication.WindowsIntegratedSecurity
        };

        var result = await UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None);

        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
    }

    [Test]
    public async Task AddWindowsAuthentication()
    {
        var options = new Options
        {
            Authentication = Authentication.WindowsAuthentication,
            Username = "user\\domain"
        };

        var result = await UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None);

        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
    }

    [Test]
    public void ThrowWhenAddWindowsAuthenticationWithInvalidUsername()
    {
        var options = new Options
        {
            Authentication = Authentication.WindowsAuthentication,
            Username = "userWithoutDomain"
        };

        Assert.ThrowsAsync<ArgumentException>(() => UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None));
    }

    [Test]
    public async Task AddClientCertiface()
    {
        string certPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..", "Test_files", "certwithpk.pfx"));
        string certPassword = "password";
        var thumprint = CertificateHandler.Handle(certPath, certPassword, false, null);

        var options = new Options
        {
            Authentication = Authentication.ClientCertificate,
            CertificateThumbprint = thumprint
        };

        var result = await UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None);

        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
    }

    [Test]
    public void ThrowWhenInvalidClientCertificate()
    {
        var options = new Options
        {
            Authentication = Authentication.ClientCertificate,
            CertificateThumbprint = "thumbprint"
        };

        Assert.ThrowsAsync<FileNotFoundException>(() => UploadFileTask.UploadFile(defaultInput, options, CancellationToken.None));
    }
}