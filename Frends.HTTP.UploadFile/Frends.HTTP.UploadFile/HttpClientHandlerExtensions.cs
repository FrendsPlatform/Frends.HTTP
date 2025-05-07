using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Frends.HTTP.UploadFile.Definitions;

namespace Frends.HTTP.UploadFile;

/// <summary>
/// Provides extension methods for various types, allowing for additional functionality to be added to existing types.
/// </summary>
internal static class HttpClientHandlerExtensions
{
    internal static void SetHandleSettingsBasedOnOptions(this HttpClientHandler handler, Options options)
    {
        switch (options.Authentication)
        {
            case Authentication.WindowsIntegratedSecurity:
                handler.UseDefaultCredentials = true;
                break;
            case Authentication.WindowsAuthentication:
                var domainAndUserName = options.Username.Split('\\');
                if (domainAndUserName.Length != 2)
                {
                    throw new ArgumentException($@"Username needs to be 'domain\username' now it was '{options.Username}'");
                }
                handler.Credentials = new NetworkCredential(domainAndUserName[1], options.Password, domainAndUserName[0]);
                break;
            case Authentication.ClientCertificate:
                handler.ClientCertificates.Add(GetCertificate(options.CertificateThumbprint));
                break;
        }

        handler.AllowAutoRedirect = options.FollowRedirects;

        if (options.AllowInvalidCertificate)
        {
            handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
        }
    }

    internal static X509Certificate2 GetCertificate(string thumbprint)
    {
        thumbprint = Regex.Replace(thumbprint, @"[^\da-zA-z]", string.Empty).ToUpper();
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
            store.Open(OpenFlags.ReadOnly);
            var signingCert = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (signingCert.Count == 0)
            {
                throw new FileNotFoundException($"Certificate with thumbprint: '{thumbprint}' not found in current user cert store.");
            }

            return signingCert[0];
        }
        finally
        {
            store.Close();
        }
    }
}
