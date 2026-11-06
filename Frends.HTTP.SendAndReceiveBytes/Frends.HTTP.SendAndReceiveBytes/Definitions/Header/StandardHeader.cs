namespace Frends.HTTP.SendAndReceiveBytes.Definitions.Header;

/// <summary>
/// Request header.
/// </summary>
public class StandardHeader : IHeader
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>Content-Type</example>
    public string Name { get; set; }

    /// <summary>
    /// Value of header.
    /// </summary>
    /// <example>application/json</example>
    public string Value { get; set; }
}
