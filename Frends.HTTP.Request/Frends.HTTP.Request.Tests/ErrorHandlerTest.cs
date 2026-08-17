using System;
using System.Threading;
using Frends.HTTP.Request.Definitions;
using NUnit.Framework;

namespace Frends.HTTP.Request.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string InvalidUrl = "http://thisdomaindoesnotexist.invalid/";
    private const string CustomErrorMessage = "CustomErrorMessage";

    private static Input InvalidInput() => new Input
    {
        Method = Method.GET,
        Url = InvalidUrl,
        Headers = Array.Empty<Header>(),
        Message = string.Empty,
    };

    private static Options DefaultOptions() => new Options
    {
        ConnectionTimeoutSeconds = 5,
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await HTTP.Request(InvalidInput(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await HTTP.Request(InvalidInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Error.AdditionalInfo, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await HTTP.Request(InvalidInput(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain(CustomErrorMessage));
    }
}
