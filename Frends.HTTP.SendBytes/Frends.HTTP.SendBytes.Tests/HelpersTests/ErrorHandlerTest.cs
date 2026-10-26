using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.SendBytes.Definitions;
using NUnit.Framework;

namespace Frends.HTTP.SendBytes.Tests.HelpersTests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.CatchAsync<Exception>(async () =>
            await HTTP.SendBytes(InvalidInput(), new Options(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = await HTTP.SendBytes(InvalidInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await HTTP.SendBytes(InvalidInput(), options, CancellationToken.None));
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input InvalidInput() => new()
    {
        Method = Method.POST,
        Url = string.Empty,
        Headers = Array.Empty<Header>(),
        ContentBytes = Array.Empty<byte>(),
    };
}
