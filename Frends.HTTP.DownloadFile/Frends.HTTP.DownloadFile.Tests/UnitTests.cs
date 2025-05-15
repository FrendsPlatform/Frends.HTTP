using Frends.HTTP.DownloadFile.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pluralsight.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Frends.HTTP.DownloadFile.Tests;

[TestClass]
public class UnitTests
{
    private static readonly string _directory = Path.Combine(Environment.CurrentDirectory, "testfiles");
    private static readonly string _filePath = Path.Combine(_directory, "picture.jpg");
    private static readonly string _targetFileAddress = @"https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Google_2015_logo.svg/1200px-Google_2015_logo.svg.png";
    private readonly string _certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "certwithpk.pfx");
    private readonly string _privateKeyPassword = "password";

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
    public async Task TestFileDownload_WithoutHeaders_AllTrue()
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
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = true,
                    LoadEntireChainForCertificate = true,
                    Password = "",
                    ThrowExceptionOnErrorResponse = true,
                    Token = "",
                    Username = "domain\\username"
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNotNull(result.FilePath);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(_directory);
            }
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithoutHeaders_AllFalse()
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
                    AllowInvalidCertificate = false,
                    AllowInvalidResponseContentTypeCharSet = false,
                    Authentication = auth,
                    AutomaticCookieHandling = false,
                    CertificateThumbprint = "",
                    ClientCertificateFilePath = "",
                    ClientCertificateInBase64 = "",
                    ClientCertificateKeyPhrase = "",
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = false,
                    LoadEntireChainForCertificate = false,
                    Password = "",
                    ThrowExceptionOnErrorResponse = false,
                    Token = "",
                    Username = "domain\\username"
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(_directory);
            }
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithHeaders()
    {
        var headers = new[] { new Header() { Name = "foo", Value = "bar" } };

        var auths = new List<Authentication>() { Authentication.None, Authentication.Basic, Authentication.WindowsAuthentication, Authentication.WindowsIntegratedSecurity, Authentication.OAuth };

        var certSource = new List<CertificateSource>() { CertificateSource.CertificateStore, CertificateSource.File, CertificateSource.String };

        var input = new Input
        {
            Url = _targetFileAddress,
            FilePath = _filePath,
            Headers = headers
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
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = false,
                    LoadEntireChainForCertificate = false,
                    Password = "",
                    ThrowExceptionOnErrorResponse = false,
                    Token = "",
                    Username = "domain\\username"
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(_directory);
            }
        }
    }

    [TestMethod]
    public async Task TestFileDownload_Certification()
    {
        var certSources = new List<CertificateSource>() { CertificateSource.File, CertificateSource.String, CertificateSource.CertificateStore };

        var input = new Input
        {
            Url = _targetFileAddress,
            FilePath = _filePath,
            Headers = null
        };

        foreach (var cert in certSources)
        {
            var tp = CertificateHandler(_certificatePath, _privateKeyPassword, false, null);

            var options = new Options
            {
                AllowInvalidCertificate = true,
                AllowInvalidResponseContentTypeCharSet = true,
                Authentication = Authentication.ClientCertificate,
                AutomaticCookieHandling = true,
                CertificateThumbprint = tp,
                ClientCertificateFilePath = _certificatePath,
                ClientCertificateInBase64 = cert is CertificateSource.String ? Convert.ToBase64String(File.ReadAllBytes(_certificatePath)) : "",
                ClientCertificateKeyPhrase = _privateKeyPassword,
                ClientCertificateSource = cert,
                ConnectionTimeoutSeconds = 60,
                FollowRedirects = true,
                LoadEntireChainForCertificate = true,
                Password = "",
                ThrowExceptionOnErrorResponse = true,
                Token = "",
                Username = "domain\\username"
            };

            var result = await HTTP.DownloadFile(input, options, default);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.FilePath);
            Assert.IsTrue(File.Exists(result.FilePath));

            Cleanup();
            Directory.CreateDirectory(_directory);
            CertificateHandler(_certificatePath, _privateKeyPassword, true, tp);
        }

    }

    private static string CertificateHandler(string path, string password, bool cleanUp, string thumbPrint)
    {
        try
        {
            if (!cleanUp)
            {
                using CryptContext ctx = new();
                ctx.Open();

                X509Certificate2 cert = ctx.CreateSelfSignedCertificate(
                    new SelfSignedCertProperties
                    {
                        IsPrivateKeyExportable = true,
                        KeyBitLength = 4096,
                        Name = new X500DistinguishedName("cn=localhost"),
                        ValidFrom = DateTime.Today.AddDays(-1),
                        ValidTo = DateTime.Today.AddMinutes(1),
                    });

                byte[] certData = cert.Export(X509ContentType.Pfx, password);

                using (X509Store store = new(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                }

                File.WriteAllBytes(path, certData);
                return cert.Thumbprint;
            }
            else
            {
                using (X509Store store = new(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

                    foreach (var cert in col)
                        store.Remove(cert);
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithOverwriteTrue_ShouldOverwriteExistingFile()
    {
        var input = new Input
        {
            Url = _targetFileAddress,
            FilePath = _filePath,
            Headers = null
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            AllowInvalidResponseContentTypeCharSet = true,
            Authentication = Authentication.None,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "",
            ClientCertificateFilePath = "",
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = "",
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = true,
            Password = "",
            ThrowExceptionOnErrorResponse = true,
            Token = "",
            Username = "domain\\username",
            Overwrite = true
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.FilePath);
        Assert.IsTrue(File.Exists(result.FilePath));

        var actualContent = File.ReadAllText(_filePath);
        Assert.AreNotEqual("OLD CONTENT", actualContent, "File should have been overwritten.");
    }
}