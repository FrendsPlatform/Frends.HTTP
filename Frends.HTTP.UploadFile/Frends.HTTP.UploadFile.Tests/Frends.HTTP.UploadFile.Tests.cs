<<<<<<< HEAD
using Frends.HTTP.UploadFile.Definitions;
=======
>>>>>>> 53d927a6357435b7526d4760bf9e519a40c06893
using HttpMock;
using HttpMock.Verify.NUnit;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

<<<<<<< HEAD
namespace Frends.HTTP.UploadFile.Tests;

[TestFixture]
public class UnitTest
{
    private IHttpServer _stubHttp;

    [SetUp]
    public void Setup()
    {
        _stubHttp = HttpMockRepository.At("http://localhost:9191");
    }

    [Test]
    public async Task RequestShouldSetEncodingWithContentTypeCharsetIgnoringCase()
    {
        var filePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..", "Test_files", "test_file.txt"));
        var codePageName = "iso-8859-1";
        var utf8ByteArray = File.ReadAllBytes(filePath);
        var expectedContentType = $"text/plain; charset={codePageName}";

        _stubHttp.Stub(x => x.Post("/endpoint"))
            .AsContentType($"text/plain; charset={codePageName}")
            .Return("foo едц")
            .OK();

        var contentType = new Header { Name = "cONTENT-tYpE", Value = expectedContentType };
        var input = new Input { Method = Method.POST, Url = "http://localhost:9191/endpoint", Headers = new Header[1] { contentType }, FilePath = filePath };
        var options = new Options { ConnectionTimeoutSeconds = 60 };
        var result = (Response)await UploadFileTask.UploadFile(input, options, CancellationToken.None);
        var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
        var requestHead = request.RequestHead;
        var requestBodyByteArray = Encoding.GetEncoding(codePageName).GetBytes(request.Body);
        var requestContentType = requestHead.Headers["cONTENT-tYpE"];

        //Casing should not affect setting header.
        Assert.That(requestContentType, Is.EqualTo(expectedContentType));
        Assert.That(requestBodyByteArray, Is.EqualTo(utf8ByteArray));
=======
namespace Frends.HTTP.UploadFile.Tests
{
    [TestFixture]
    class TestClass
    {
        [TestFixture]
        public class UnitTest
        {
            private IHttpServer _stubHttp;

            [SetUp]
            public void Setup()
            {
                _stubHttp = HttpMockRepository.At("http://localhost:9191");
            }

            [Test]
            public async Task RequestShouldSetEncodingWithContentTypeCharsetIgnoringCase()
            {
                var fileLocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../..", "Test_files", "test_file.txt"));
                var codePageName = "iso-8859-1";
                var utf8ByteArray = File.ReadAllBytes(fileLocation);
                var expectedContentType = $"text/plain; charset={codePageName}";

                _stubHttp.Stub(x => x.Post("/endpoint"))
                    .AsContentType($"text/plain; charset={codePageName}")
                    .Return("foo едц")
                    .OK();

                var contentType = new Header { Name = "cONTENT-tYpE", Value = expectedContentType };
                var input = new Input { Method = Method.POST, Url = "http://localhost:9191/endpoint", Headers = new Header[1] { contentType }, FileLocation = fileLocation };
                var options = new Options { ConnectionTimeoutSeconds = 60 };
                var result = (Response)await UploadFileTask.UploadFile(input, options, CancellationToken.None);
                var request = _stubHttp.AssertWasCalled(called => called.Post("/endpoint")).LastRequest();
                var requestHead = request.RequestHead;
                var requestBodyByteArray = Encoding.GetEncoding(codePageName).GetBytes(request.Body);
                var requestContentType = requestHead.Headers["cONTENT-tYpE"];

                //Casing should not affect setting header.
                Assert.That(requestContentType, Is.EqualTo(expectedContentType));
                Assert.That(requestBodyByteArray, Is.EqualTo(utf8ByteArray));
            }
        }
>>>>>>> 53d927a6357435b7526d4760bf9e519a40c06893
    }
}