namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Certificate store location.
/// </summary>
public enum CertificateStoreLocation
{
    /// <summary>
    /// The X.509 certificate store assigned to the current user.
    /// </summary>
    CurrentUser,
    /// <summary>
    /// The X.509 certificate store assigned to the local machine.
    /// </summary>
    LocalMachine
}
