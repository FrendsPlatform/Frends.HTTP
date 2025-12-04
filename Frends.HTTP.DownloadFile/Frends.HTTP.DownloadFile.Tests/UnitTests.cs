using Frends.HTTP.DownloadFile.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Pluralsight.Crypto;

namespace Frends.HTTP.DownloadFile.Tests;

[TestClass]
public class UnitTests
{
    private static readonly string _directory = Path.Combine(Environment.CurrentDirectory, "testfiles");
    private static readonly string _filePath = Path.Combine(_directory, "picture.jpg");

    private static readonly string _targetFileAddress =
        "https://frendsfonts.blob.core.windows.net/images/frendsLogo.png";//@"http://localhost:9999/testfile.png";
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

        var auths = new List<Authentication>
        {
            Authentication.None,
            Authentication.Basic,
            Authentication.WindowsAuthentication,
            Authentication.WindowsIntegratedSecurity,
            Authentication.OAuth
        };

        var certSource = new List<CertificateSource>
        {
            CertificateSource.CertificateStore,
            CertificateSource.File,
            CertificateSource.String
        };

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
        var certSources = new List<CertificateSource>
        {
            CertificateSource.File,
            CertificateSource.String,
            CertificateSource.CertificateStore
        };

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
        File.WriteAllText(_filePath, "OLD CONTENT");

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

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task TestFileDownload_WithEmptyUrl_ShouldThrowException()
    {
        var input = new Input
        {
            Url = "",
            FilePath = _filePath,
            Headers = null
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            Authentication = Authentication.None,
            ConnectionTimeoutSeconds = 60
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [TestMethod]
    public async Task TestFileDownload_WithCertificateStoreLocation_CurrentUser()
    {
        var tp = CertificateHandler(_certificatePath, _privateKeyPassword, false, null);

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
            Authentication = Authentication.ClientCertificate,
            AutomaticCookieHandling = true,
            CertificateThumbprint = tp,
            ClientCertificateFilePath = _certificatePath,
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = _privateKeyPassword,
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = CertificateStoreLocation.CurrentUser,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
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

        CertificateHandler(_certificatePath, _privateKeyPassword, true, tp);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task TestFileDownload_WithCertificateStoreLocation_LocalMachine_NotFound()
    {
        // Use real HTTP client factory to test certificate lookup failure
        HTTP.ClientFactory = new HttpClientFactory();

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
            Authentication = Authentication.ClientCertificate,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "NONEXISTENTTHUMBPRINT",
            ClientCertificateFilePath = "",
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = "",
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = CertificateStoreLocation.LocalMachine,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "",
            ThrowExceptionOnErrorResponse = true,
            Token = "",
            Username = "domain\\username"
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [TestMethod]
    public async Task TestFileDownload_WithOverwriteFalse_ExistingFile_ShouldThrow()
    {
        File.WriteAllText(_filePath, "OLD CONTENT");

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
            Overwrite = false
        };

        await Assert.ThrowsExceptionAsync<Exception>(async () =>
            await HTTP.DownloadFile(input, options, default));
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public async Task TestFileDownload_WindowsAuth_InvalidUsername_ShouldThrow()
    {
        // Use real HTTP client factory to test username validation
        HTTP.ClientFactory = new HttpClientFactory();

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
            Authentication = Authentication.WindowsAuthentication,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "",
            ClientCertificateFilePath = "",
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = "",
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "password",
            ThrowExceptionOnErrorResponse = true,
            Token = "",
            Username = "invalid_username_without_domain"
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [TestMethod]
    public async Task TestFileDownload_CertificateFromFile_WithoutKeyPhrase()
    {
        var tp = CertificateHandler(_certificatePath, _privateKeyPassword, false, null);

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
            Authentication = Authentication.ClientCertificate,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "",
            ClientCertificateFilePath = _certificatePath,
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = _privateKeyPassword,
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "",
            ThrowExceptionOnErrorResponse = true,
            Token = "",
            Username = "domain\\username"
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        CertificateHandler(_certificatePath, _privateKeyPassword, true, tp);
    }

    [TestMethod]
    public async Task TestFileDownload_CertificateFromString_WithKeyPhrase()
    {
        var tp = CertificateHandler(_certificatePath, _privateKeyPassword, false, null);

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
            Authentication = Authentication.ClientCertificate,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "",
            ClientCertificateFilePath = "",
            ClientCertificateInBase64 = Convert.ToBase64String(File.ReadAllBytes(_certificatePath)),
            ClientCertificateKeyPhrase = _privateKeyPassword,
            ClientCertificateSource = CertificateSource.String,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "",
            ThrowExceptionOnErrorResponse = true,
            Token = "",
            Username = "domain\\username"
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        CertificateHandler(_certificatePath, _privateKeyPassword, true, tp);
    }

    [TestMethod]
    public async Task TestFileDownload_WithNullHeaders()
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
            Authentication = Authentication.Basic,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "",
            ClientCertificateFilePath = "",
            ClientCertificateInBase64 = "",
            ClientCertificateKeyPhrase = "",
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "password",
            ThrowExceptionOnErrorResponse = false,
            Token = "",
            Username = "domain\\username"
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task TestFileDownload_CachedClient()
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
            LoadEntireChainForCertificate = false,
            Password = "",
            ThrowExceptionOnErrorResponse = false,
            Token = "",
            Username = ""
        };

        // First request creates client
        var result1 = await HTTP.DownloadFile(input, options, default);
        Assert.IsTrue(result1.Success);

        // Cleanup and recreate directory for second request
        Cleanup();
        Directory.CreateDirectory(_directory);

        // Second request should use cached client
        var result2 = await HTTP.DownloadFile(input, options, default);
        Assert.IsTrue(result2.Success);
    }
}