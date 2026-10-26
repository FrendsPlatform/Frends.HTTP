using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.HTTP.DownloadFile.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.HTTP.DownloadFile.Tests;

[TestClass]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [TestMethod]
    public async Task ShouldThrowErrorWhenThrowErrorOnFailureIsTrue()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
            HTTP.DownloadFile(InvalidInput(), new Options(), CancellationToken.None));
    }

    [TestMethod]
    public async Task ShouldReturnFailedResultWhenThrowErrorOnFailureIsFalse()
    {
        var options = new Options { ThrowErrorOnFailure = false };

        var result = await HTTP.DownloadFile(InvalidInput(), options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsNotNull(result.Error.AdditionalInfo);
    }

    [TestMethod]
    public async Task ShouldUseCustomErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };

        var ex = await Assert.ThrowsExceptionAsync<Exception>(() =>
            HTTP.DownloadFile(InvalidInput(), options, CancellationToken.None));

        StringAssert.Contains(ex.Message, CustomErrorMessage);
    }

    private static Input InvalidInput() => new() { Url = string.Empty, FilePath = "file.txt" };
}
