using System.ComponentModel;

namespace Frends.HTTP.RequestBytes.Definitions.Header;

/// <summary>
/// Request header, with value treated as a secret.
/// </summary>
public class SecretHeader : IHeader
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>x-api-key</example>
    public string Name { get; set; }

    /// <summary>
    /// Value of header treated as a secret.
    /// </summary>
    /// <example>my-secret-key</example>
    [PasswordPropertyText]
    public string Value { get; set; }
}
