using Frends.HTTP.DownloadFile.Definitions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Frends.HTTP.DownloadFile;

internal static class Extensions
{
    internal static void SetHandlerSettingsBasedOnOptions(this HttpClientHandler handler, Options options)
    {
        switch (options.Authentication)
        {
            case Authentication.WindowsIntegratedSecurity:
                handler.UseDefaultCredentials = true;
                break;
            case Authentication.WindowsAuthentication:
                var domainAndUserName = options.Username.Split('\\');
                if (domainAndUserName.Length != 2)
                    throw new ArgumentException($@"Username needs to be 'domain\username' now it was '{options.Username}'");

                handler.Credentials =
                    new NetworkCredential(domainAndUserName[1], options.Password, domainAndUserName[0]);
                break;
            case Authentication.ClientCertificate:
                handler.ClientCertificates.AddRange(GetCertificates(options));
                break;
        }

        handler.AllowAutoRedirect = options.FollowRedirects;
        handler.UseCookies = options.AutomaticCookieHandling;

        if (options.AllowInvalidCertificate)
            handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
    }

    internal static void SetDefaultRequestHeadersBasedOnOptions(this HttpClient httpClient, Options options)
    {
        //Do not automatically set expect 100-continue response header
        httpClient.DefaultRequestHeaders.ExpectContinue = false;
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/json");
        httpClient.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(options.ConnectionTimeoutSeconds));
    }

    private static X509Certificate[] GetCertificates(Options options)
    {
        X509Certificate2[] certificates;

        switch (options.ClientCertificateSource)
        {
            case CertificateSource.CertificateStore:
                var thumbprint = options.CertificateThumbprint;
                certificates = GetCertificatesFromStore(thumbprint, options.LoadEntireChainForCertificate);
                break;
            case CertificateSource.File:
                certificates = GetCertificatesFromFile(options.ClientCertificateFilePath, options.ClientCertificateKeyPhrase);
                break;
            case CertificateSource.String:
                certificates = GetCertificatesFromString(options.ClientCertificateInBase64, options.ClientCertificateKeyPhrase);
                break;
            default:
                throw new Exception("Unsupported Certificate source");
        }

        return certificates.Cast<X509Certificate>().ToArray();
    }

    private static X509Certificate2[] GetCertificatesFromString(string certificateContentsBase64, string keyPhrase)
    {
        var certificateBytes = Convert.FromBase64String(certificateContentsBase64);
        return LoadCertificatesFromBytes(certificateBytes, keyPhrase);
    }

    // Have to skip these because there's no certificate to use for unit testing.
    [ExcludeFromCodeCoverage]
    private static X509Certificate2[] LoadCertificatesFromBytes(byte[] certificateBytes, string keyPhrase)
    {
        var collection = new X509Certificate2Collection();

        if (!string.IsNullOrEmpty(keyPhrase))
            collection.Import(certificateBytes, keyPhrase, X509KeyStorageFlags.PersistKeySet);
        else
            collection.Import(certificateBytes, null, X509KeyStorageFlags.PersistKeySet);

        return collection.Cast<X509Certificate2>().OrderByDescending(c => c.HasPrivateKey).ToArray();

    }

    private static X509Certificate2[] GetCertificatesFromFile(string clientCertificateFilePath, string keyPhrase)
    {
        return LoadCertificatesFromBytes(File.ReadAllBytes(clientCertificateFilePath), keyPhrase);
    }

    // Have to skip these because there's no certificate to use for unit testing.
    [ExcludeFromCodeCoverage]
    private static X509Certificate2[] GetCertificatesFromStore(string thumbprint,
        bool loadEntireChain)
    {
        thumbprint = Regex.Replace(thumbprint, @"[^\da-zA-z]", string.Empty).ToUpper();
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var signingCert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        if (signingCert.Count == 0)
            throw new FileNotFoundException($"Certificate with thumbprint: '{thumbprint}' not found in current user cert store.");

        var certificate = signingCert[0];

        if (!loadEntireChain)
            return new[] { certificate };

        var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.Build(certificate);

        // include the whole chain
        var certificates = chain
            .ChainElements.Cast<X509ChainElement>()
            .Select(c => c.Certificate)
            .OrderByDescending(c => c.HasPrivateKey)
            .ToArray();

        return certificates;
    }
}