using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Frends.HTTP.DownloadFile.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Definitions = Frends.HTTP.DownloadFile.Definitions;

namespace Frends.HTTP.DownloadFile.Tests;

[TestClass]
public class UnitTests
{
    private static readonly string TestDirectory = Path.Combine(Environment.CurrentDirectory, "testfiles");
    private static readonly string TestFilePath = Path.Combine(TestDirectory, "picture.jpg");

    private static readonly string TargetFileAddress =
        "https://frendsfonts.blob.core.windows.net/images/frendsLogo.png";

    private readonly string certificatePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "certwithpk.pfx");

    private readonly string privateKeyPassword = "password";

    [TestInitialize]
    public void TestInitialize()
    {
        HTTP.ClearClientCache();
        Directory.CreateDirectory(TestDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(TestDirectory))
            Directory.Delete(TestDirectory, true);
    }

    [TestMethod]
    public async Task DownloadFileShouldReuseCachedClientForSameOptions()
    {
        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };
        var opt = new Options
        {
            Authentication = Authentication.Basic,
            Username = "cache-test-username",
            Password = "cache-test-password",
            ProxyUsername = "cache-test-proxy-username",
            ProxyPassword = "cache-test-proxy-password",
            ClientCertificateKeyPhrase = "cache-test-certificate-key-phrase",
            ConnectionTimeoutSeconds = 60,
            Overwrite = true,
        };

        await HTTP.DownloadFile(input, opt, default);
        await HTTP.DownloadFile(input, opt, default);

        Assert.AreEqual(1, MemoryCache.Default.GetCount());

        foreach (var cacheEntry in MemoryCache.Default)
        {
            Assert.IsFalse(cacheEntry.Key.Contains("cache-test-username"));
            Assert.IsFalse(cacheEntry.Key.Contains("cache-test-password"));
            Assert.IsFalse(cacheEntry.Key.Contains("cache-test-proxy-username"));
            Assert.IsFalse(cacheEntry.Key.Contains("cache-test-proxy-password"));
            Assert.IsFalse(cacheEntry.Key.Contains("cache-test-certificate-key-phrase"));
        }

        opt.Password = "other";
        await HTTP.DownloadFile(input, opt, default);

        Assert.AreEqual(2, MemoryCache.Default.GetCount());
    }

    [TestMethod]
    public async Task TestFileDownload_WithoutHeaders_AllTrue()
    {
        var auths = new List<Authentication>()
        {
            Authentication.None,
            Authentication.Basic,
            Authentication.WindowsAuthentication,
            Authentication.WindowsIntegratedSecurity,
            Authentication.OAuth,
        };

        var certSource = new List<CertificateSource>()
        {
            CertificateSource.CertificateStore,
            CertificateSource.File,
            CertificateSource.String,
        };

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
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
                    CertificateThumbprint = string.Empty,
                    ClientCertificateFilePath = string.Empty,
                    ClientCertificateInBase64 = string.Empty,
                    ClientCertificateKeyPhrase = string.Empty,
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = true,
                    LoadEntireChainForCertificate = true,
                    Password = string.Empty,
                    ThrowExceptionOnErrorResponse = true,
                    Token = string.Empty,
                    Username = "domain\\username",
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNotNull(result.FilePath);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(TestDirectory);
            }
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithoutHeaders_AllFalse()
    {
        var auths = new List<Authentication>()
        {
            Authentication.None,
            Authentication.Basic,
            Authentication.WindowsAuthentication,
            Authentication.WindowsIntegratedSecurity,
            Authentication.OAuth,
        };

        var certSource = new List<CertificateSource>()
        {
            CertificateSource.CertificateStore,
            CertificateSource.File,
            CertificateSource.String,
        };

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
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
                    CertificateThumbprint = string.Empty,
                    ClientCertificateFilePath = string.Empty,
                    ClientCertificateInBase64 = string.Empty,
                    ClientCertificateKeyPhrase = string.Empty,
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = false,
                    LoadEntireChainForCertificate = false,
                    Password = string.Empty,
                    ThrowExceptionOnErrorResponse = false,
                    Token = string.Empty,
                    Username = "domain\\username",
                };

                var result = await HTTP.DownloadFile(input, options, default);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsTrue(File.Exists(result.FilePath));

                Cleanup();
                Directory.CreateDirectory(TestDirectory);
            }
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithHeaders()
    {
        var headers = new[]
        {
            new Header
            {
                Name = "foo",
                Value = "bar",
            },
        };

        var auths = new List<Authentication>
        {
            Authentication.None,

            // Authentication.Basic,
            // Authentication.WindowsAuthentication,
            // Authentication.WindowsIntegratedSecurity,
            // Authentication.OAuth
        };

        var certSource = new List<CertificateSource>
        {
            CertificateSource.CertificateStore,
            CertificateSource.File,
            CertificateSource.String,
        };

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = headers,
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
                    CertificateThumbprint = string.Empty,
                    ClientCertificateFilePath = string.Empty,
                    ClientCertificateInBase64 = string.Empty,
                    ClientCertificateKeyPhrase = string.Empty,
                    ClientCertificateSource = cert,
                    ConnectionTimeoutSeconds = 60,
                    FollowRedirects = false,
                    LoadEntireChainForCertificate = false,
                    Password = string.Empty,
                    ThrowExceptionOnErrorResponse = false,
                    Token = string.Empty,
                    Username = "domain\\username",
                };

                try
                {
                    var result = await HTTP.DownloadFile(input, options, default);

                    Assert.IsNotNull(result);
                    Assert.IsTrue(result.Success);
                    Assert.IsTrue(File.Exists(result.FilePath));
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Authentication: {auth}; Certificate Source: {cert}; Error: {ex.Message}");
                }

                Cleanup();
                Directory.CreateDirectory(TestDirectory);
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
            CertificateSource.CertificateStore,
        };

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        foreach (var cert in certSources)
        {
            var tp = CertificateHandler(certificatePath, privateKeyPassword, false, null);

            var options = new Options
            {
                AllowInvalidCertificate = true,
                AllowInvalidResponseContentTypeCharSet = true,
                Authentication = Authentication.ClientCertificate,
                AutomaticCookieHandling = true,
                CertificateThumbprint = tp,
                ClientCertificateFilePath = certificatePath,
                ClientCertificateInBase64 =
                    cert is CertificateSource.String ? Convert.ToBase64String(File.ReadAllBytes(certificatePath)) : string.Empty,
                ClientCertificateKeyPhrase = privateKeyPassword,
                ClientCertificateSource = cert,
                ConnectionTimeoutSeconds = 60,
                FollowRedirects = true,
                LoadEntireChainForCertificate = true,
                Password = string.Empty,
                ThrowExceptionOnErrorResponse = true,
                Token = string.Empty,
                Username = "domain\\username",
            };

            var result = await HTTP.DownloadFile(input, options, default);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.FilePath);
            Assert.IsTrue(File.Exists(result.FilePath));

            Cleanup();
            Directory.CreateDirectory(TestDirectory);
            CertificateHandler(certificatePath, privateKeyPassword, true, tp);
        }
    }

    [TestMethod]
    public async Task TestFileDownload_WithOverwriteTrue_ShouldOverwriteExistingFile()
    {
        File.WriteAllText(TestFilePath, "OLD CONTENT");

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            AllowInvalidResponseContentTypeCharSet = true,
            Authentication = Authentication.None,
            AutomaticCookieHandling = true,
            CertificateThumbprint = string.Empty,
            ClientCertificateFilePath = string.Empty,
            ClientCertificateInBase64 = string.Empty,
            ClientCertificateKeyPhrase = string.Empty,
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = true,
            Password = string.Empty,
            ThrowExceptionOnErrorResponse = true,
            Token = string.Empty,
            Username = "domain\\username",
            Overwrite = true,
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.FilePath);
        Assert.IsTrue(File.Exists(result.FilePath));

        var actualContent = File.ReadAllText(TestFilePath);
        Assert.AreNotEqual("OLD CONTENT", actualContent, "File should have been overwritten.");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task TestFileDownload_WithEmptyUrl_ShouldThrowException()
    {
        var input = new Input
        {
            Url = string.Empty,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            Authentication = Authentication.None,
            ConnectionTimeoutSeconds = 60,
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
    public async Task TestFileDownload_WithCertificateStoreLocation_LocalMachine_NotFound()
    {
        // Use real HTTP client factory to test certificate lookup failure
        HTTP.ClientFactory = new HttpClientFactory();

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            AllowInvalidResponseContentTypeCharSet = true,
            Authentication = Authentication.ClientCertificate,
            AutomaticCookieHandling = true,
            CertificateThumbprint = "NONEXISTENTTHUMBPRINT",
            ClientCertificateFilePath = string.Empty,
            ClientCertificateInBase64 = string.Empty,
            ClientCertificateKeyPhrase = string.Empty,
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = CertificateStoreLocation.LocalMachine,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = string.Empty,
            ThrowExceptionOnErrorResponse = true,
            Token = string.Empty,
            Username = "domain\\username",
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [TestMethod]
    public async Task TestFileDownload_WithOverwriteFalse_ExistingFile_ShouldThrow()
    {
        File.WriteAllText(TestFilePath, "OLD CONTENT");

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            AllowInvalidResponseContentTypeCharSet = true,
            Authentication = Authentication.None,
            AutomaticCookieHandling = true,
            CertificateThumbprint = string.Empty,
            ClientCertificateFilePath = string.Empty,
            ClientCertificateInBase64 = string.Empty,
            ClientCertificateKeyPhrase = string.Empty,
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = true,
            Password = string.Empty,
            ThrowExceptionOnErrorResponse = true,
            Token = string.Empty,
            Username = "domain\\username",
            Overwrite = false,
        };

        await Assert.ThrowsExceptionAsync<IOException>(async () =>
            await HTTP.DownloadFile(input, options, default));
    }

    [TestMethod]
    [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
    public async Task TestFileDownload_WindowsAuth_InvalidUsername_ShouldThrow()
    {
        // Use real HTTP client factory to test username validation
        HTTP.ClientFactory = new HttpClientFactory();

        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            AllowInvalidCertificate = true,
            AllowInvalidResponseContentTypeCharSet = true,
            Authentication = Authentication.WindowsAuthentication,
            AutomaticCookieHandling = true,
            CertificateThumbprint = string.Empty,
            ClientCertificateFilePath = string.Empty,
            ClientCertificateInBase64 = string.Empty,
            ClientCertificateKeyPhrase = string.Empty,
            ClientCertificateSource = CertificateSource.File,
            ConnectionTimeoutSeconds = 60,
            FollowRedirects = true,
            LoadEntireChainForCertificate = false,
            Password = "password",
            ThrowExceptionOnErrorResponse = true,
            Token = string.Empty,
            Username = "invalid_username_without_domain",
        };

        await HTTP.DownloadFile(input, options, default);
    }

    [DataTestMethod]
    [DataRow(CertificateStoreLocation.CurrentUser, "current user")]
    [DataRow(CertificateStoreLocation.LocalMachine, "local machine")]
    public void CorrectStoreSearched(CertificateStoreLocation storeLocation, string storeLocationText)
    {
        var handler = new HttpClientHandler();
        var options = new Options
        {
            Authentication = Authentication.ClientCertificate,
            ClientCertificateSource = CertificateSource.CertificateStore,
            CertificateStoreLocation = storeLocation,
            CertificateThumbprint = "InvalidThumbprint",
        };
        var ex = Assert.ThrowsExactly<FileNotFoundException>(() =>
            handler.SetHandlerSettingsBasedOnOptions(options));

        Assert.IsNotNull(ex);
        StringAssert.Contains(
            ex.Message,
            $"Certificate with thumbprint: 'INVALIDTHUMBPRINT' not found in {storeLocationText} cert store.");
    }

    [TestMethod]
    public void HandlerShouldUseConfiguredProxy()
    {
        var handler = new HttpClientHandler();
        var options = new Options
        {
            UseProxy = true,
            ProxyUrl = "http://proxy.example.com:8080",
            ProxyUsername = "proxy-user",
            ProxyPassword = "proxy-password",
        };

        handler.SetHandlerSettingsBasedOnOptions(options);

        var proxy = handler.Proxy as WebProxy;
        Assert.IsNotNull(proxy);
        Assert.IsTrue(handler.UseProxy);
        Assert.AreEqual(new Uri("http://proxy.example.com:8080/"), proxy.Address);
        var credentials = proxy.Credentials.GetCredential(proxy.Address, string.Empty);
        Assert.AreEqual("proxy-user", credentials.UserName);
        Assert.AreEqual("proxy-password", credentials.Password);
    }

    [TestMethod]
    public async Task RequestShouldSetTls12And13WhenConfigured()
    {
        var input = new Input
        {
            Url = TargetFileAddress,
            FilePath = TestFilePath,
            Headers = null,
        };

        var options = new Options
        {
            ConnectionTimeoutSeconds = 60,
            SslProtocolVersion = SslVersion.Tls12And13,
            HttpProtocolVersion = Definitions.HttpVersion.Http11,
            Overwrite = true,
        };

        var result = await HTTP.DownloadFile(input, options, default);

        Assert.IsTrue(result.Success);
    }

    private static string CertificateHandler(string path, string password, bool cleanUp, string thumbPrint)
    {
        try
        {
            if (!cleanUp)
            {
                using var rsa = RSA.Create(4096);
                var request = new CertificateRequest(
                    new X500DistinguishedName("cn=localhost"),
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                X509Certificate2 cert = request.CreateSelfSigned(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

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
                    X509Certificate2Collection col =
                        store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

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
}
