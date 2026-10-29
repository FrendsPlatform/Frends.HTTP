using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.RequestBytes.Definitions;
using NUnit.Framework;

namespace Frends.HTTP.RequestBytes.Tests;

[TestFixture]
public class ErrorHandlerTest
{
    [Test]
    public void ShouldThrowWhenThrowErrorOnFailureIsTrue()
    {
        var input = new Input { Url = string.Empty, Headers = Array.Empty<Header>() };
        var options = new Options { ThrowErrorOnFailure = true };

        Assert.ThrowsAsync<ArgumentNullException>(() => HTTP.RequestBytes(input, options, CancellationToken.None));
    }

    [Test]
    public async Task ShouldReturnFailedResultWhenThrowErrorOnFailureIsFalse()
    {
        var input = new Input { Url = string.Empty, Headers = Array.Empty<Header>() };
        var options = new Options { ThrowErrorOnFailure = false, ErrorMessageOnFailure = "Request failed" };

        var result = await HTTP.RequestBytes(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.StartWith("Request failed"));
        Assert.That(result.Error.AdditionalInfo, Is.Not.Null);
    }

    [Test]
    public void ShouldThrowCustomMessageWhenConfigured()
    {
        var input = new Input { Url = string.Empty, Headers = Array.Empty<Header>() };
        var options = new Options { ThrowErrorOnFailure = true, ErrorMessageOnFailure = "Custom" };

        var ex = Assert.ThrowsAsync<Exception>(() => HTTP.RequestBytes(input, options, CancellationToken.None));
        Assert.That(ex.Message, Is.EqualTo("Custom"));
    }
}
