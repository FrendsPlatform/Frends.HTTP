namespace Frends.HTTP.SendBytes.Definitions;

/// <summary>
/// Request header.
/// </summary>
public class Header
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>Content-Type</example>
    public string Name { get; set; }

    /// <summary>
    /// Value of header.
    /// </summary>
    /// <example>application/octet-stream</example>
    public string Value { get; set; }
}
