namespace Frends.HTTP.RequestBytes.Definitions;

/// <summary>
/// Certificate source.
/// </summary>
public enum CertificateSource
{
    /// <summary>
    /// Source is certificate store.
    /// </summary>
    CertificateStore,
    /// <summary>
    /// Source is a file.
    /// </summary>
    File,
    /// <summary>
    /// Source is a string.
    /// </summary>
    String
}
