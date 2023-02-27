namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Request header.
/// </summary>
public class Header
{
    /// <summary>
    /// Name of header.
    /// </summary>
    /// <example>foo</example>
    public string Name { get; set; }

    /// <summary>
    /// Value of header.
    /// </summary>
    /// <example>bar</example>
    public string Value { get; set; }
}
