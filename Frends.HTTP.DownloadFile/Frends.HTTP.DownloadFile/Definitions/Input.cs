using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HTTP.DownloadFile.Definitions;

/// <summary>
/// Input class
/// </summary>
public class Input
{
    /// <summary>
    /// The URL with protocol and path. 
    /// You can include query parameters directly in the URL.
    /// </summary>
    /// <example>https://example.org/path/to</example>
    [DefaultValue("https://example.org/path/to")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Url { get; set; }

    /// <summary>
    /// List of HTTP headers to be added to the request.
    /// </summary>
    /// <example>[ { Name = "Content-Type", Value = "application/json" }, { Name = "Accept", Value = "application/json"  } ]</example>
    public Header[] Headers { get; set; }

    /// <summary>
    /// Exact location and name of the file to be created.
    /// </summary>
    /// <example>C:\\temp\example.txt</example>
    public string FilePath { get; set; }
}