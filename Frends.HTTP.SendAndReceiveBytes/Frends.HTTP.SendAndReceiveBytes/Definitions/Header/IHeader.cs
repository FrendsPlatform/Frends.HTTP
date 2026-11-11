namespace Frends.HTTP.SendAndReceiveBytes.Definitions.Header;

/// <summary>
/// Interface for request header.
/// </summary>
public interface IHeader
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>Content-Type</example>
    string Name { get; set; }

    /// <summary>
    /// Value of header.
    /// </summary>
    /// <example>application/json</example>
    string Value { get; set; }
}
