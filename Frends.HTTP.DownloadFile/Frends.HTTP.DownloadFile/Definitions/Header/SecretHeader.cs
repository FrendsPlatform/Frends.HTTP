using System.ComponentModel;

namespace Frends.HTTP.DownloadFile.Definitions.Header;

/// <summary>
/// Request header, with value treated as a secret.
/// </summary>
public class SecretHeader : IHeader
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>Content-Type</example>
    public string Name { get; set; }

    /// <summary>
    /// Value of header treated as a secret.
    /// </summary>
    /// <example>application/json</example>
    [PasswordPropertyText]
    public string Value { get; set; }
}
