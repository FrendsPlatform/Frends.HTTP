namespace Frends.HTTP.UploadFile.Definitions;
/// <summary>
/// Represents an HTTP header, which consists of a name-value pair.
/// </summary>
public class Header
{
    /// <summary>
    /// The name of the header.
    /// </summary>
    /// <example>Example Name</example>
    public string Name { get; set; }

    /// <summary>
    /// The value of the header.
    /// </summary>
    /// <example>Example Value</example>
    public string Value { get; set; }

}
