using Frends.HTTP.DownloadFile.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Frends.HTTP.DownloadFile.Tests;

[TestClass]
public class UnitTests
{
    private static readonly string _directory = Path.Combine(Environment.CurrentDirectory, "testfiles");
    private static readonly string _filePath = Path.Combine(_directory, "picture.jpg");
    private static readonly string _targetFileAddress = @"https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Google_2015_logo.svg/1200px-Google_2015_logo.svg.png";

    [TestInitialize]
    public void TestInitialize()
    {
        HTTP.ClearClientCache();
        Directory.CreateDirectory(_directory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, true);
    }

    [TestMethod]
    public async Task TestFileDownload()
    {
        var auths = new List<Authentication>() { Authentication.None, Authentication.Basic, Authentication.WindowsAuthentication, Authentication.WindowsIntegratedSecurity, Authentication.OAuth };

        var certSource = new List<CertificateSource>() { CertificateSource.CertificateStore, CertificateSource.File, CertificateSource.String };

        var input = new Input
        {
            Url = _targetFileAddress,
            FilePath = _filePath,
            Headers = null
        };

        foreach (var auth in auths)
        {
            foreach (var cert in certSource)
            {
                var options = new Options
                {
                    AllowInvalidCertificate = true,
                    AllowInvalidResponseContentTypeCharSet = true,
                    Authentication = auth,
                    AutomaticCookieHandling = true,
                    CertificateThumbprint = "",
                    ClientCertificateFilePath = "",
                    ClientCertificateInBase64 = "",
                    ClientCertificateKeyPhrase = "",
                    ClientCertificateSource = CertificateSource.String,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = false,
                    LoadEntireChainForCertificate = false,
                    Password = "",
                    ThrowExceptionOnErrorResponse = false,
                    Token = "",
                    Username = "domain\\username"
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsTrue(result != null);
                Assert.IsNotNull(result.FilePath);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(_directory);
            }
        }
    }
}