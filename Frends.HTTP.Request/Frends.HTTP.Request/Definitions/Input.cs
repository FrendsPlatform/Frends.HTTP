using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.HTTP.Request.Definitions;

/// <summary>
/// Input class
/// </summary>
public class Input
{
    /// <summary>
    /// The HTTP Method to be used with the request.
    /// </summary>
    /// <example>GET</example>
    public Method Method { get; set; }

    /// <summary>
    /// The URL with protocol and path. You can include query parameters directly in the url.
    /// </summary>
    /// <example>https://example.org/path/to</example>
    [DefaultValue("https://example.org/path/to")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Url { get; set; }

    /// <summary>
    /// The message text to be sent with the request.
    /// </summary>
    /// <example>{ "Body": "Message" }</example>
    [UIHint(nameof(Method), "", Method.POST, Method.DELETE, Method.PATCH, Method.PUT)]
    public string Message { get; set; }

    /// <summary>
    /// List of HTTP headers to be added to the request.
    /// </summary>
    /// <example>Name: Header, Value: HeaderValue</example>
    public Header[] Headers { get; set; }
}
