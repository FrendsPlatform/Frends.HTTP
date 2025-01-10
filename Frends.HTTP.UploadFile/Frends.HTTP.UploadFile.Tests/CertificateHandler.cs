using Pluralsight.Crypto;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Frends.HTTP.UploadFile.Tests;

public static class CertificateHandler
{
    public static string Handle(string path, string password, bool cleanUp, string thumbPrint)
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
                using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);

                foreach (var cert in col)
                    store.Remove(cert);

                return null;
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}